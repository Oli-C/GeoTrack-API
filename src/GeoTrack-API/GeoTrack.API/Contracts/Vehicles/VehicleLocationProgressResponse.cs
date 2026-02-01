using System.Text.Json.Serialization;

namespace GeoTrack.API.Contracts.Vehicles;

/// <summary>
/// Combined view: latest known position plus a progress summary over a time window.
/// </summary>
public sealed class VehicleLocationProgressResponse
{
    /// <summary>
    /// The vehicle identifier these values relate to.
    /// </summary>
    [JsonPropertyName("vehicleId")]
    public Guid VehicleId { get; set; }

    /// <summary>
    /// The latest known position for the vehicle, or <see langword="null"/> if no location has been received yet.
    /// </summary>
    [JsonPropertyName("latest")]
    public VehicleLatestPositionResponse? Latest { get; set; }

    /// <summary>
    /// Summary computed over the requested time window.
    /// </summary>
    [JsonPropertyName("summary")]
    public VehicleProgressSummaryResponse Summary { get; set; } = new();
}

/// <summary>
/// Latest known position for a vehicle.
/// </summary>
public sealed class VehicleLatestPositionResponse
{
    /// <summary>
    /// Latitude in decimal degrees.
    /// </summary>
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude in decimal degrees.
    /// </summary>
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    /// <summary>
    /// Device timestamp for the position (UTC). This may arrive out-of-order.
    /// </summary>
    [JsonPropertyName("timestampUtc")]
    public DateTime TimestampUtc { get; set; }

    /// <summary>
    /// Server receive time for the position (UTC).
    /// </summary>
    [JsonPropertyName("receivedAtUtc")]
    public DateTime ReceivedAtUtc { get; set; }

    /// <summary>
    /// Speed in km/h, if supplied by the device.
    /// </summary>
    [JsonPropertyName("speedKph")]
    public double? SpeedKph { get; set; }

    /// <summary>
    /// Heading in degrees in the range [0, 360), if supplied by the device.
    /// </summary>
    [JsonPropertyName("headingDegrees")]
    public double? HeadingDegrees { get; set; }

    /// <summary>
    /// Horizontal accuracy estimate in meters, if supplied by the device.
    /// </summary>
    [JsonPropertyName("accuracyMeters")]
    public double? AccuracyMeters { get; set; }
}

/// <summary>
/// Summary over a time window of recent points.
/// </summary>
public sealed class VehicleProgressSummaryResponse
{
    /// <summary>
    /// Inclusive window start (UTC) used by the summary query.
    /// </summary>
    [JsonPropertyName("windowStartUtc")]
    public DateTime WindowStartUtc { get; set; }

    /// <summary>
    /// Inclusive window end (UTC) used by the summary query.
    /// </summary>
    [JsonPropertyName("windowEndUtc")]
    public DateTime WindowEndUtc { get; set; }

    /// <summary>
    /// Number of points considered in the window.
    /// </summary>
    [JsonPropertyName("pointsCount")]
    public int PointsCount { get; set; }

    /// <summary>
    /// Approximate distance travelled within the window in meters.
    /// Computed as the sum of great-circle (Haversine) distances between successive points.
    /// </summary>
    [JsonPropertyName("distanceMeters")]
    public double DistanceMeters { get; set; }

    /// <summary>
    /// Average speed in km/h for points that reported speed, or <see langword="null"/> when no speed values are present.
    /// </summary>
    [JsonPropertyName("avgSpeedKph")]
    public double? AvgSpeedKph { get; set; }

    /// <summary>
    /// Whether the vehicle is considered stale based on <c>secondsSinceLastUpdate</c> compared to the requested threshold.
    /// </summary>
    [JsonPropertyName("isStale")]
    public bool IsStale { get; set; }

    /// <summary>
    /// Seconds since the last received update (based on <c>latest.receivedAtUtc</c>), or <see langword="null"/> when no latest point exists.
    /// </summary>
    [JsonPropertyName("secondsSinceLastUpdate")]
    public int? SecondsSinceLastUpdate { get; set; }
}
