using GeoTrack.Domain.Vehicles;
using GeoTrack.API.Contracts.Vehicles;
using GeoTrack.API.Data;
using GeoTrack.API.Data.Entities;
using GeoTrack.API.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeoTrack.API.Contracts;

namespace GeoTrack.API.Controllers;

[ApiController]
[Route("vehicles")]
public sealed class VehiclesController : ControllerBase
{
    private readonly TrackingDbContext _db;
    private readonly TenantContext _tenant;

    public VehiclesController(TrackingDbContext db, TenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpPost]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VehicleResponse>> Create([FromBody] CreateVehicleRequest request, CancellationToken ct)
    {
        if (!_tenant.HasTenant)
            return BadRequest(new ApiErrorResponse { Error = ApiErrors.Tenant.MissingCode, Message = $"{ApiErrors.Tenant.MissingMessage} '{ApiHeaders.TenantId}'." });

        var tenantId = _tenant.TenantId;

        var registrationNumber = NormalizeOrNull(request.RegistrationNumber);
        var name = NormalizeOrNull(request.Name);
        var externalId = NormalizeOrNull(request.ExternalId);

        // NOTE: Field length validation is handled via DataAnnotations on the request contract.

        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle(
            tenantId: tenantId,
            id: vehicleId,
            identity: new VehicleIdentity(registrationNumber, name, externalId),
            createdAtUtc: DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc));

        _db.Vehicles.Add(vehicle);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsDuplicateRegistration(ex))
        {
            return Conflict(new ApiErrorResponse
            {
                Error = ApiErrors.Vehicles.DuplicateRegistrationNumberCode,
                Message = ApiErrors.Vehicles.DuplicateRegistrationNumberMessage
            });
        }

        var response = ToResponse(vehicle);

        return CreatedAtAction(nameof(GetById), new { vehicleId = vehicle.Id }, response);
    }

    [HttpGet("{vehicleId:guid}")]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleResponse>> GetById([FromRoute] Guid vehicleId, CancellationToken ct)
    {
        if (!_tenant.HasTenant)
            return BadRequest(new ApiErrorResponse { Error = ApiErrors.Tenant.MissingCode, Message = $"{ApiErrors.Tenant.MissingMessage} '{ApiHeaders.TenantId}'." });

        var tenantId = _tenant.TenantId;

        var vehicle = await _db.Vehicles
            .AsNoTracking()
            .SingleOrDefaultAsync(v => v.TenantId == tenantId && v.Id == vehicleId, ct);

        if (vehicle is null)
            return NotFound(new ApiErrorResponse { Error = ApiErrors.GpsFixes.VehicleNotFoundCode, Message = ApiErrors.GpsFixes.VehicleNotFoundMessage });

        return Ok(ToResponse(vehicle));
    }

    // Optional: list (paged)
    [HttpGet]
    [ProducesResponseType(typeof(PagedVehiclesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedVehiclesResponse>> List(
        [FromQuery] int page = ApiValidation.Paging.DefaultPage,
        [FromQuery] int pageSize = ApiValidation.Paging.DefaultPageSize,
        CancellationToken ct = default)
    {
        if (!_tenant.HasTenant)
            return BadRequest(new ApiErrorResponse { Error = ApiErrors.Tenant.MissingCode, Message = $"{ApiErrors.Tenant.MissingMessage} '{ApiHeaders.TenantId}'." });

        var tenantId = _tenant.TenantId;

        if (page < ApiValidation.Paging.MinPage)
            return BadRequest(new ApiErrorResponse { Error = ApiErrors.Validation.InvalidPagingCode, Message = ApiValidation.Paging.PageMessage });

        if (pageSize is < ApiValidation.Paging.MinPageSize or > ApiValidation.Paging.MaxPageSize)
            return BadRequest(new ApiErrorResponse { Error = ApiErrors.Validation.InvalidPagingCode, Message = ApiValidation.Paging.PageSizeMessage });

        var query = _db.Vehicles
            .AsNoTracking()
            .Where(v => v.TenantId == tenantId);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(v => v.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Ok(new PagedVehiclesResponse
        {
            Items = items.Select(ToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = total
        });
    }

    // Optional: patch identity/status
    [HttpPatch("{vehicleId:guid}")]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VehicleResponse>> Patch([FromRoute] Guid vehicleId, [FromBody] PatchVehicleRequest request, CancellationToken ct)
    {
        if (!_tenant.HasTenant)
            return BadRequest(new ApiErrorResponse { Error = ApiErrors.Tenant.MissingCode, Message = $"{ApiErrors.Tenant.MissingMessage} '{ApiHeaders.TenantId}'." });

        var tenantId = _tenant.TenantId;

        var vehicle = await _db.Vehicles
            .SingleOrDefaultAsync(v => v.TenantId == tenantId && v.Id == vehicleId, ct);

        if (vehicle is null)
            return NotFound(new ApiErrorResponse { Error = ApiErrors.GpsFixes.VehicleNotFoundCode, Message = ApiErrors.GpsFixes.VehicleNotFoundMessage });

        var registrationNumber = request.RegistrationNumber is null ? null : NormalizeOrNull(request.RegistrationNumber);
        var name = request.Name is null ? null : NormalizeOrNull(request.Name);
        var externalId = request.ExternalId is null ? null : NormalizeOrNull(request.ExternalId);

        // NOTE: Field length validation is handled via DataAnnotations on the request contract.

        // Identity updates: only apply fields provided (null means "set to null" when explicitly present)
        if (request.RegistrationNumber is not null)
            vehicle.SetRegistration(registrationNumber);

        if (request.Name is not null)
            vehicle.Rename(name);

        if (request.ExternalId is not null)
            vehicle.SetExternalId(externalId);

        if (request.Status is not null)
        {
            // Enum value validity is handled via DataAnnotations; we keep the switch for lifecycle rules.
            switch (request.Status.Value)
            {
                case VehicleStatus.Active:
                    vehicle.Activate();
                    break;
                case VehicleStatus.Inactive:
                    vehicle.SetInactive();
                    break;
                case VehicleStatus.Decommissioned:
                    vehicle.Decommission();
                    break;
            }
        }

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsDuplicateRegistration(ex))
        {
            return Conflict(new ApiErrorResponse
            {
                Error = ApiErrors.Vehicles.DuplicateRegistrationNumberCode,
                Message = ApiErrors.Vehicles.DuplicateRegistrationNumberMessage
            });
        }

        return Ok(ToResponse(vehicle));
    }

    // -----------------------------
    // Vehicle Summary
    // -----------------------------

    /// <summary>
    /// Returns a detailed summary of a vehicle, including identity/status, latest location (if any), and progress over a recent window.
    /// </summary>
    /// <param name="vehicleId">Vehicle identifier.</param>
    /// <param name="windowMinutes">Progress window size in minutes (1..1440). Default 60.</param>
    /// <param name="staleAfterSeconds">Marks <c>latestLocation.isStale</c> when the latest update is older than this many seconds (1..86400). Default 300.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns the vehicle summary.</response>
    /// <response code="400">Missing/invalid tenant header or invalid query parameters.</response>
    /// <response code="404">Vehicle not found for the tenant.</response>
    [HttpGet("{vehicleId:guid}/summary")]
    [ProducesResponseType(typeof(VehicleSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleSummaryResponse>> GetSummary(
        [FromRoute] Guid vehicleId,
        [FromQuery] int windowMinutes = ApiValidation.VehicleProgress.DefaultWindowMinutes,
        [FromQuery] int staleAfterSeconds = ApiValidation.VehicleProgress.DefaultStaleAfterSeconds,
        CancellationToken ct = default)
    {
        if (!_tenant.HasTenant)
            return BadRequest(new ApiErrorResponse { Error = ApiErrors.Tenant.MissingCode, Message = $"{ApiErrors.Tenant.MissingMessage} '{ApiHeaders.TenantId}'." });

        var tenantId = _tenant.TenantId;

        if (windowMinutes is < ApiValidation.VehicleProgress.MinWindowMinutes or > ApiValidation.VehicleProgress.MaxWindowMinutes)
            return BadRequest(new ApiErrorResponse { Error = ApiErrors.Validation.InvalidQueryCode, Message = ApiValidation.VehicleProgress.WindowMinutesMessage });

        if (staleAfterSeconds is < ApiValidation.VehicleProgress.MinStaleAfterSeconds or > ApiValidation.VehicleProgress.MaxStaleAfterSeconds)
            return BadRequest(new ApiErrorResponse { Error = ApiErrors.Validation.InvalidQueryCode, Message = ApiValidation.VehicleProgress.StaleAfterSecondsMessage });

        var vehicle = await _db.Vehicles
            .AsNoTracking()
            .SingleOrDefaultAsync(v => v.TenantId == tenantId && v.Id == vehicleId, ct);

        if (vehicle is null)
            return NotFound(new ApiErrorResponse { Error = ApiErrors.GpsFixes.VehicleNotFoundCode, Message = ApiErrors.GpsFixes.VehicleNotFoundMessage });

        var latest = await _db.VehicleLatestLocations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.VehicleId == vehicleId, ct);

        var nowUtc = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        VehicleLatestLocationSummaryResponse? latestLocation = null;
        if (latest is not null)
        {
            var secondsSince = (int)Math.Max(0, (nowUtc - latest.ReceivedAtUtc).TotalSeconds);
            latestLocation = new VehicleLatestLocationSummaryResponse
            {
                Latitude = latest.Latitude,
                Longitude = latest.Longitude,
                DeviceTimeUtc = latest.DeviceTimeUtc,
                ReceivedAtUtc = latest.ReceivedAtUtc,
                SecondsSinceLastUpdate = secondsSince,
                IsStale = secondsSince > staleAfterSeconds
            };
        }

        // Progress window
        var windowEndUtc = nowUtc;
        var windowStartUtc = windowEndUtc.AddMinutes(-windowMinutes);

        // vehicle_location_progress table has been removed.
        // We keep the response contract stable by returning an empty progress window summary.
        var pointsCount = 0;
        var distanceMeters = 0.0;
        double? avgSpeedKph = null;

        var response = new VehicleSummaryResponse
        {
            VehicleId = vehicle.Id,
            RegistrationNumber = vehicle.Identity?.RegistrationNumber,
            Name = vehicle.Identity?.Name,
            Status = vehicle.Status,
            LatestLocation = latestLocation,
            Progress = new VehicleProgressSummaryResponse
            {
                WindowStartUtc = windowStartUtc,
                WindowEndUtc = windowEndUtc,
                PointsCount = pointsCount,
                DistanceMeters = distanceMeters,
                AvgSpeedKph = avgSpeedKph
            }
        };

        return Ok(response);
    }

    // -----------------------------
    // Latest location (dedicated)
    // -----------------------------

    /// <summary>
    /// Returns only the latest known location of a vehicle.
    /// </summary>
    /// <param name="vehicleId">Vehicle identifier.</param>
    /// <param name="staleAfterSeconds">Marks <c>isStale</c> when the latest update is older than this many seconds (1..86400). Default 300.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns the latest location.</response>
    /// <response code="204">Vehicle exists, but no location has been recorded yet.</response>
    /// <response code="400">Missing/invalid tenant header or invalid query parameters.</response>
    /// <response code="404">Vehicle not found for the tenant.</response>
    [HttpGet("{vehicleId:guid}/latest-location")]
    [ProducesResponseType(typeof(VehicleLatestLocationSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleLatestLocationSummaryResponse?>> GetLatestLocation(
        [FromRoute] Guid vehicleId,
        [FromQuery] int staleAfterSeconds = ApiValidation.VehicleProgress.DefaultStaleAfterSeconds,
        CancellationToken ct = default)
    {
        if (!_tenant.HasTenant)
            return BadRequest(new ApiErrorResponse { Error = ApiErrors.Tenant.MissingCode, Message = $"{ApiErrors.Tenant.MissingMessage} '{ApiHeaders.TenantId}'." });

        var tenantId = _tenant.TenantId;

        if (staleAfterSeconds is < ApiValidation.VehicleProgress.MinStaleAfterSeconds or > ApiValidation.VehicleProgress.MaxStaleAfterSeconds)
            return BadRequest(new ApiErrorResponse { Error = ApiErrors.Validation.InvalidQueryCode, Message = ApiValidation.VehicleProgress.StaleAfterSecondsMessage });

        // Ensure vehicle exists for this tenant.
        var vehicle = await _db.Vehicles
            .AsNoTracking()
            .SingleOrDefaultAsync(v => v.TenantId == tenantId && v.Id == vehicleId, ct);

        if (vehicle is null)
            return NotFound(new ApiErrorResponse { Error = ApiErrors.GpsFixes.VehicleNotFoundCode, Message = ApiErrors.GpsFixes.VehicleNotFoundMessage });

        var latest = await _db.VehicleLatestLocations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.VehicleId == vehicleId, ct);
        if (latest is null)
            return NoContent();

        var nowUtc = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        var secondsSince = (int)Math.Max(0, (nowUtc - latest.ReceivedAtUtc).TotalSeconds);

        return Ok(new VehicleLatestLocationSummaryResponse
        {
            Latitude = latest.Latitude,
            Longitude = latest.Longitude,
            DeviceTimeUtc = latest.DeviceTimeUtc,
            ReceivedAtUtc = latest.ReceivedAtUtc,
            SecondsSinceLastUpdate = secondsSince,
            IsStale = secondsSince > staleAfterSeconds
        });
    }


    private static VehicleResponse ToResponse(Vehicle v) => new()
    {
        Id = v.Id,
        RegistrationNumber = v.Identity?.RegistrationNumber,
        Name = v.Identity?.Name,
        ExternalId = v.Identity?.ExternalId,
        CreatedAtUtc = v.CreatedAtUtc,
        Status = v.Status
    };

    private static string? NormalizeOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }

    private static bool IsDuplicateRegistration(DbUpdateException ex)
    {
        // For Npgsql, the inner exception is typically PostgresException with SqlState 23505.
        // We also match the known index name to avoid false positives.
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("23505", StringComparison.Ordinal)
               && message.Contains("ux_vehicle_tenant_registration_number", StringComparison.OrdinalIgnoreCase);
    }

    private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6371000; // metres
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2))
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return r * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);
}
