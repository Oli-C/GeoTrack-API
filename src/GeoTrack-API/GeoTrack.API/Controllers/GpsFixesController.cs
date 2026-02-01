using System;
using System.Collections.Generic;
using System.Linq;
using GeoTrack.Domain.Common.ValueObjects;
using GeoTrack.Domain.Vehicles;
using GeoTrack.API.Common;
using GeoTrack.API.Contracts.GpsFixes;
using GeoTrack.API.Contracts.Vehicles;
using GeoTrack.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeoTrack.API.Contracts;

namespace GeoTrack.API.Controllers;

[ApiController]
public sealed class GpsFixesController : ControllerBase
{
    private readonly TrackingDbContext _db;
    private readonly TenantContext _tenant;

    public GpsFixesController(TrackingDbContext db, TenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    /// <summary>
    /// POST /vehicles/{id}/gps-fixes - for single updates or near real-time sending.
    /// </summary>
    [HttpPost("vehicles/{vehicleId:guid}/gps-fixes")]
    [ProducesResponseType(typeof(IngestGpsFixResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IngestGpsFixResponse>> IngestSingle(
        [FromRoute] Guid vehicleId,
        [FromBody] IngestGpsFixRequest request,
        CancellationToken ct)
    {
        if (!_tenant.HasTenant)
            return BadRequest(new ApiErrorResponse { Error = ApiErrors.Tenant.MissingCode, Message = $"{ApiErrors.Tenant.MissingMessage} '{ApiHeaders.TenantId}'." });

        var tenantId = _tenant.TenantId;

        // Ensure vehicle exists for tenant.
        var vehicleExists = await _db.Vehicles
            .AsNoTracking()
            .AnyAsync(v => v.TenantId == tenantId && v.Id == vehicleId, ct);

        if (!vehicleExists)
            return NotFound(new ApiErrorResponse { Error = ApiErrors.GpsFixes.VehicleNotFoundCode, Message = ApiErrors.GpsFixes.VehicleNotFoundMessage });

        if (request.DeviceTimeUtc.Kind != DateTimeKind.Utc)
            return BadRequest(new ApiErrorResponse { Error = ApiErrors.GpsFixes.InvalidDeviceTimeCode, Message = ApiErrors.GpsFixes.InvalidDeviceTimeMessage });

        if (request.HeadingDegrees is >= ApiValidation.GpsFixes.HeadingDegreesMaxExclusive)
            return BadRequest(new ApiErrorResponse { Error = ApiErrors.GpsFixes.InvalidHeadingCode, Message = ApiErrors.GpsFixes.InvalidHeadingMessage });

        if (string.IsNullOrWhiteSpace(request.CorrelationId))
            return BadRequest(new ApiErrorResponse { Error = ApiErrors.GpsFixes.MissingCorrelationIdCode, Message = ApiErrors.GpsFixes.MissingCorrelationIdMessage });

        var nowUtc = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        var gpsFixId = Guid.NewGuid();
        var gpsFix = new GpsFix(
            id: gpsFixId,
            tenantId: tenantId,
            vehicleId: vehicleId,
            latitude: Latitude.From(request.Latitude),
            longitude: Longitude.From(request.Longitude),
            deviceTimeUtc: request.DeviceTimeUtc,
            receivedAtUtc: nowUtc,
            source: TelemetrySource.Device,
            deviceSequence: DeviceSequence.FromNullable(request.DeviceSequence),
            speed: SpeedKph.FromNullable(request.SpeedKph),
            heading: HeadingDegrees.FromNullable(request.HeadingDegrees),
            accuracy: AccuracyMeters.FromNullable(request.AccuracyMeters),
            altitudeMeters: AltitudeMeters.FromNullable(request.AltitudeMeters),
            odometerKm: OdometerKm.FromNullable(request.OdometerKm),
            correlationId: CorrelationId.From(request.CorrelationId),
            qualityOverride: FixQuality.Unknown);

        _db.GpsFixes.Add(gpsFix);

        var isLatestApplied = await UpsertLatestLocationIfNewer(tenantId, vehicleId, gpsFix, ct);

        await _db.SaveChangesAsync(ct);

        return Created($"/vehicles/{vehicleId}/gps-fixes/{gpsFixId}", new IngestGpsFixResponse
        {
            VehicleId = vehicleId,
            GpsFixId = gpsFixId,
            DeviceTimeUtc = gpsFix.DeviceTimeUtc,
            ReceivedAtUtc = gpsFix.ReceivedAtUtc,
            IsLatestApplied = isLatestApplied
        });
    }

    /// <summary>
    /// POST /gps-fixes/batch - for devices that buffer and send in batches.
    /// </summary>
    [HttpPost("gps-fixes/batch")]
    [ProducesResponseType(typeof(BatchIngestGpsFixesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchIngestGpsFixesResponse>> IngestBatch(
        [FromBody] BatchIngestGpsFixesRequest request,
        CancellationToken ct)
    {
        if (!_tenant.HasTenant)
            return BadRequest(new ApiErrorResponse { Error = ApiErrors.Tenant.MissingCode, Message = $"{ApiErrors.Tenant.MissingMessage} '{ApiHeaders.TenantId}'." });

        var tenantId = _tenant.TenantId;

        var receivedAtUtc = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        var requestedVehicleIds = request.Items
            .Select(i => i.VehicleId)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var tenantVehicleIds = await _db.Vehicles
            .AsNoTracking()
            .Where(v => v.TenantId == tenantId && requestedVehicleIds.Contains(v.Id))
            .Select(v => v.Id)
            .ToListAsync(ct);

        var tenantVehicleIdSet = tenantVehicleIds.ToHashSet();

        var results = new List<BatchIngestGpsFixesResponse.ItemResult>(request.Items.Count);
        var accepted = 0;
        var rejected = 0;

        var bestPerVehicle = new Dictionary<Guid, GpsFix>();

        for (var index = 0; index < request.Items.Count; index++)
        {
            var item = request.Items[index];

            if (item.VehicleId == Guid.Empty)
            {
                rejected++;
                results.Add(Rejected(index, item.VehicleId, ApiErrors.GpsFixes.InvalidVehicleCode, ApiErrors.GpsFixes.InvalidVehicleMessage));
                continue;
            }

            if (!tenantVehicleIdSet.Contains(item.VehicleId))
            {
                rejected++;
                results.Add(Rejected(index, item.VehicleId, ApiErrors.GpsFixes.VehicleNotFoundCode, ApiErrors.GpsFixes.VehicleNotFoundMessage));
                continue;
            }

            if (item.DeviceTimeUtc.Kind != DateTimeKind.Utc)
            {
                rejected++;
                results.Add(Rejected(index, item.VehicleId, ApiErrors.GpsFixes.InvalidDeviceTimeCode, ApiErrors.GpsFixes.InvalidDeviceTimeMessage));
                continue;
            }

            if (item.HeadingDegrees is >= ApiValidation.GpsFixes.HeadingDegreesMaxExclusive)
            {
                rejected++;
                results.Add(Rejected(index, item.VehicleId, ApiErrors.GpsFixes.InvalidHeadingCode, ApiErrors.GpsFixes.InvalidHeadingMessage));
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.CorrelationId))
            {
                rejected++;
                results.Add(Rejected(index, item.VehicleId, ApiErrors.GpsFixes.MissingCorrelationIdCode, ApiErrors.GpsFixes.MissingCorrelationIdMessage));
                continue;
            }

            try
            {
                var fixId = Guid.NewGuid();
                var gpsFix = new GpsFix(
                    id: fixId,
                    tenantId: tenantId,
                    vehicleId: item.VehicleId,
                    latitude: Latitude.From(item.Latitude),
                    longitude: Longitude.From(item.Longitude),
                    deviceTimeUtc: item.DeviceTimeUtc,
                    receivedAtUtc: receivedAtUtc,
                    source: TelemetrySource.Device,
                    deviceSequence: DeviceSequence.FromNullable(item.DeviceSequence),
                    speed: SpeedKph.FromNullable(item.SpeedKph),
                    heading: HeadingDegrees.FromNullable(item.HeadingDegrees),
                    accuracy: AccuracyMeters.FromNullable(item.AccuracyMeters),
                    altitudeMeters: AltitudeMeters.FromNullable(item.AltitudeMeters),
                    odometerKm: OdometerKm.FromNullable(item.OdometerKm),
                    correlationId: CorrelationId.From(item.CorrelationId),
                    qualityOverride: FixQuality.Unknown);

                _db.GpsFixes.Add(gpsFix);

                accepted++;
                results.Add(new BatchIngestGpsFixesResponse.ItemResult
                {
                    Index = index,
                    VehicleId = item.VehicleId,
                    Status = ApiErrors.GpsFixes.AcceptedStatus,
                    GpsFixId = fixId
                });

                if (!bestPerVehicle.TryGetValue(item.VehicleId, out var best) || LatestLocationPolicy.IsNewer(gpsFix, best))
                    bestPerVehicle[item.VehicleId] = gpsFix;
            }
            catch (Exception ex)
            {
                rejected++;
                results.Add(Rejected(index, item.VehicleId, ApiErrors.GpsFixes.InvalidPayloadCode, ex.Message));
            }
        }

        foreach (var (vehicleId, gpsFix) in bestPerVehicle)
        {
            await UpsertLatestLocationIfNewer(tenantId, vehicleId, gpsFix, ct);
        }

        await _db.SaveChangesAsync(ct);

        return Ok(new BatchIngestGpsFixesResponse
        {
            AcceptedCount = accepted,
            RejectedCount = rejected,
            ReceivedAtUtc = receivedAtUtc,
            Results = results
        });
    }

    private async Task<bool> UpsertLatestLocationIfNewer(Guid tenantId, Guid vehicleId, GpsFix gpsFix, CancellationToken ct)
    {
        var current = await _db.VehicleLatestLocations
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.VehicleId == vehicleId, ct);

        if (!LatestLocationPolicy.ShouldReplace(current, gpsFix))
            return false;

        // Domain entity has private setters; easiest update pattern is replace.
        var preservedRouteScheduleId = current?.RouteScheduleId;

        if (current is not null)
            _db.VehicleLatestLocations.Remove(current);

        _db.VehicleLatestLocations.Add(LatestLocationPolicy.CreateSnapshot(
            tenantId: tenantId,
            vehicleId: vehicleId,
            gpsFix: gpsFix,
            routeScheduleId: preservedRouteScheduleId,
            updatedAtUtc: DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)));

        return true;
    }

    private static BatchIngestGpsFixesResponse.ItemResult Rejected(int index, Guid vehicleId, string error, string message) =>
        new()
        {
            Index = index,
            VehicleId = vehicleId,
            Status = ApiErrors.GpsFixes.RejectedStatus,
            Error = error,
            Message = message
        };
}
