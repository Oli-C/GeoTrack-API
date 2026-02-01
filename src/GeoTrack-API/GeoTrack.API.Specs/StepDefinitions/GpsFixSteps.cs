using System.Net.Http.Json;
using GeoTrack.Domain.Common.ValueObjects;
using GeoTrack.Domain.Vehicles;
using GeoTrack.API.Common;
using GeoTrack.API.Contracts.GpsFixes;
using GeoTrack.API.Contracts.Vehicles;
using GeoTrack.API.Data;
using GeoTrack.API.Specs.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Xunit;

namespace GeoTrack.API.Specs.StepDefinitions;

[Binding]
public sealed class GpsFixSteps
{
    private readonly GeoTrackApiFactory _factory;
    private readonly ScenarioContext _scenarioContext;

    public GpsFixSteps(GeoTrackApiFactory factory, ScenarioContext scenarioContext)
    {
        _factory = factory;
        _scenarioContext = scenarioContext;
    }

    private HttpClient Client => _scenarioContext.Get<HttpClient>(nameof(HttpClient));

    private TrackingDbContext Db => _factory.Services.GetRequiredService<TrackingDbContext>();

    private Guid TenantId
    {
        get => _scenarioContext.Get<Guid>(nameof(TenantId));
        set => _scenarioContext[nameof(TenantId)] = value;
    }

    [Given("I have a tenant {string}")]
    public async Task GivenIHaveATenant(string tenantId)
    {
        TenantId = Guid.Parse(tenantId);

        Client.DefaultRequestHeaders.Remove(ApiHeaders.TenantId);
        Client.DefaultRequestHeaders.Add(ApiHeaders.TenantId, TenantId.ToString());

        // Seed tenant
        var exists = await Db.Tenants.AnyAsync(t => t.Id == TenantId);
        if (!exists)
        {
            Db.Tenants.Add(new GeoTrack.API.Data.Entities.Tenant(TenantId, "Test Tenant", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)));
            await Db.SaveChangesAsync();
        }
    }

    [Given("I have a vehicle {string} for that tenant")]
    public async Task GivenIHaveAVehicleForThatTenant(string vehicleId)
    {
        var id = Guid.Parse(vehicleId);

        var exists = await Db.Vehicles.AnyAsync(v => v.TenantId == TenantId && v.Id == id);
        if (!exists)
        {
            Db.Vehicles.Add(new GeoTrack.API.Data.Entities.Vehicle(
                tenantId: TenantId,
                id: id,
                identity: new GeoTrack.API.Data.Entities.VehicleIdentity("REG-1", "Test Vehicle", null),
                createdAtUtc: DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)));
            await Db.SaveChangesAsync();
        }
    }

    [Given("the vehicle {string} has a latest location at {string}")]
    public async Task GivenTheVehicleHasALatestLocationAt(string vehicleId, string deviceTimeUtc)
    {
        var vId = Guid.Parse(vehicleId);
        var deviceTime = DateTime.SpecifyKind(DateTime.Parse(deviceTimeUtc), DateTimeKind.Utc);
        var receivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        // Ensure vehicle exists
        var exists = await Db.Vehicles.AnyAsync(v => v.TenantId == TenantId && v.Id == vId);
        if (!exists)
        {
            Db.Vehicles.Add(new GeoTrack.API.Data.Entities.Vehicle(
                tenantId: TenantId,
                id: vId,
                identity: new GeoTrack.API.Data.Entities.VehicleIdentity("REG-1", "Test Vehicle", null),
                createdAtUtc: DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)));
        }

        // Replace any existing latest location
        var current = await Db.VehicleLatestLocations.SingleOrDefaultAsync(x => x.TenantId == TenantId && x.VehicleId == vId);
        if (current is not null)
            Db.VehicleLatestLocations.Remove(current);

        // Seed via domain policy so the snapshot mapping stays consistent with production code.
        var seedFix = new GpsFix(
            id: Guid.NewGuid(),
            vehicleId: vId,
            latitude: Latitude.From(51.0),
            longitude: Longitude.From(-0.1),
            deviceTimeUtc: deviceTime,
            receivedAtUtc: receivedAt,
            source: TelemetrySource.Device,
            deviceSequence: DeviceSequence.FromNullable(0),
            speed: null,
            heading: null,
            accuracy: null,
            altitudeMeters: null,
            odometerKm: null,
            quality: FixQuality.Unknown,
            correlationId: CorrelationId.From("spec-seed"));

        Db.VehicleLatestLocations.Add(LatestLocationPolicy.CreateSnapshot(
            tenantId: TenantId,
            vehicleId: vId,
            gpsFix: seedFix,
            routeScheduleId: null,
            updatedAtUtc: DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)));

        await Db.SaveChangesAsync();
    }

    [When("I POST a single GPS fix for vehicle {string} with device time {string}")]
    public async Task WhenIPostASingleGpsFixForVehicleWithDeviceTime(string vehicleId, string deviceTimeUtc)
    {
        var vId = Guid.Parse(vehicleId);

        var payload = new IngestGpsFixRequest
        {
            Latitude = 51.0,
            Longitude = -0.1,
            DeviceTimeUtc = DateTime.SpecifyKind(DateTime.Parse(deviceTimeUtc), DateTimeKind.Utc),
            DeviceSequence = 1,
            SpeedKph = 10,
            HeadingDegrees = 45,
            AccuracyMeters = 5,
            AltitudeMeters = 100,
            OdometerKm = 123.4,
            CorrelationId = "spec-single"
        };

        var response = await Client.PostAsJsonAsync($"/vehicles/{vId}/gps-fixes", payload);
        _scenarioContext[nameof(HttpResponseMessage)] = response;

        if (response.Content.Headers.ContentType?.MediaType == "application/json")
        {
            var body = await response.Content.ReadFromJsonAsync<IngestGpsFixResponse>();
            _scenarioContext[nameof(IngestGpsFixResponse)] = body!;
        }
    }

    [When("I POST a batch of 2 gps fixes where 1 is for an unknown vehicle")]
    public async Task WhenIPostABatchOfGpsFixesWhereOneIsUnknown()
    {
        // Known vehicle from background
        var knownVehicleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var unknownVehicleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var payload = new BatchIngestGpsFixesRequest
        {
            Items =
            {
                new BatchIngestGpsFixesRequest.BatchGpsFixItem
                {
                    VehicleId = knownVehicleId,
                    Latitude = 51.0,
                    Longitude = -0.1,
                    DeviceTimeUtc = DateTime.SpecifyKind(DateTime.Parse("2026-01-30T12:00:00Z"), DateTimeKind.Utc),
                    DeviceSequence = 1,
                    SpeedKph = 12,
                    HeadingDegrees = 10,
                    AccuracyMeters = 5,
                    CorrelationId = "spec-batch-1"
                },
                new BatchIngestGpsFixesRequest.BatchGpsFixItem
                {
                    VehicleId = unknownVehicleId,
                    Latitude = 52.0,
                    Longitude = -0.2,
                    DeviceTimeUtc = DateTime.SpecifyKind(DateTime.Parse("2026-01-30T12:00:01Z"), DateTimeKind.Utc),
                    DeviceSequence = 2,
                    SpeedKph = 13,
                    HeadingDegrees = 11,
                    AccuracyMeters = 6,
                    CorrelationId = "spec-batch-2"
                }
            }
        };

        var response = await Client.PostAsJsonAsync("/gps-fixes/batch", payload);
        _scenarioContext[nameof(HttpResponseMessage)] = response;

        var body = await response.Content.ReadFromJsonAsync<BatchIngestGpsFixesResponse>();
        _scenarioContext[nameof(BatchIngestGpsFixesResponse)] = body!;
    }

    [Then("the vehicle {string} should have {int} gps fixes")]
    public async Task ThenTheVehicleShouldHaveGpsFixes(string vehicleId, int count)
    {
        var vId = Guid.Parse(vehicleId);
        var actual = await Db.GpsFixes.CountAsync(x => x.VehicleId == vId);
        Assert.Equal(count, actual);
    }

    [Then("the vehicle {string} latest location device time should be {string}")]
    public async Task ThenTheVehicleLatestLocationDeviceTimeShouldBe(string vehicleId, string deviceTimeUtc)
    {
        var vId = Guid.Parse(vehicleId);
        var expected = DateTime.SpecifyKind(DateTime.Parse(deviceTimeUtc), DateTimeKind.Utc);

        var latest = await Db.VehicleLatestLocations.SingleOrDefaultAsync(x => x.TenantId == TenantId && x.VehicleId == vId);
        Assert.NotNull(latest);
        Assert.Equal(expected, latest!.DeviceTimeUtc);
    }

    [Then("the batch ingest result should have {int} accepted and {int} rejected")]
    public void ThenTheBatchIngestResultShouldHaveAcceptedAndRejected(int accepted, int rejected)
    {
        var body = _scenarioContext.Get<BatchIngestGpsFixesResponse>(nameof(BatchIngestGpsFixesResponse));
        Assert.Equal(accepted, body.AcceptedCount);
        Assert.Equal(rejected, body.RejectedCount);
    }
}
