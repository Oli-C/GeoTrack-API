using System.Text.Json.Serialization;
using GeoTrack.API.Data.Entities;

namespace GeoTrack.API.Contracts.Vehicles;

/// <summary>
/// Vehicle summary view for dashboards.
/// Includes identity/status and the latest known location (if any).
/// </summary>
public sealed class VehicleSummaryResponse
{
    /// <summary>
    /// Vehicle identifier.
    /// </summary>
    [JsonPropertyName("vehicleId")]
    public Guid VehicleId { get; set; }

    /// <summary>
    /// Registration/plate identifier (if set).
    /// </summary>
    [JsonPropertyName("registrationNumber")]
    public string? RegistrationNumber { get; set; }

    /// <summary>
    /// Friendly display name (if set).
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Current lifecycle status.
    /// </summary>
    [JsonPropertyName("status")]
    public VehicleStatus Status { get; set; }

    /// <summary>
    /// Latest known location update for the vehicle, or <see langword="null"/> if none exists.
    /// </summary>
    [JsonPropertyName("latestLocation")]
    public VehicleLatestLocationSummaryResponse? LatestLocation { get; set; }

    /// <summary>
    /// Progress summary computed over a recent time window.
    /// </summary>
    [JsonPropertyName("progress")]
    public VehicleProgressSummaryResponse Progress { get; set; } = new();
}

/// <summary>
/// Latest known location used in a vehicle summary card.
/// </summary>
public sealed class VehicleLatestLocationSummaryResponse
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    /// <summary>
    /// Device-provided timestamp (UTC) for the location.
    /// </summary>
    [JsonPropertyName("deviceTimeUtc")]
    public DateTime DeviceTimeUtc { get; set; }

    /// <summary>
    /// Server receive time (UTC) for the location.
    /// </summary>
    [JsonPropertyName("receivedAtUtc")]
    public DateTime ReceivedAtUtc { get; set; }

    /// <summary>
    /// Seconds since the last received update (based on <c>receivedAtUtc</c>).
    /// </summary>
    [JsonPropertyName("secondsSinceLastUpdate")]
    public int SecondsSinceLastUpdate { get; set; }

    /// <summary>
    /// Whether the location is considered stale relative to the configured threshold.
    /// </summary>
    [JsonPropertyName("isStale")]
    public bool IsStale { get; set; }
}

// NOTE: VehicleProgressSummaryResponse is defined in VehicleLocationProgressResponse.cs and reused here.
