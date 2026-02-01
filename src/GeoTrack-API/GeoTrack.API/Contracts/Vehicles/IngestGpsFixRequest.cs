using System;
using System.ComponentModel.DataAnnotations;

namespace GeoTrack.API.Contracts.Vehicles;

/// <summary>
/// Request payload for ingesting a single GPS fix for a vehicle.
/// Use this for near real-time updates.
/// </summary>
public sealed class IngestGpsFixRequest
{
    /// <summary>
    /// Latitude in decimal degrees.
    /// </summary>
    [Range(-90, 90)]
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude in decimal degrees.
    /// </summary>
    [Range(-180, 180)]
    public double Longitude { get; set; }

    /// <summary>
    /// The device timestamp for this fix.
    /// Must be UTC (i.e. <c>DateTime.Kind == Utc</c>).
    /// </summary>
    [Required]
    public DateTime DeviceTimeUtc { get; set; }

    /// <summary>
    /// Optional device-provided monotonic sequence number used to break ties when
    /// multiple fixes have the same <see cref="DeviceTimeUtc"/>.
    /// </summary>
    public long? DeviceSequence { get; set; }

    /// <summary>
    /// Speed in kilometres per hour.
    /// </summary>
    [Range(0, double.MaxValue)]
    public double? SpeedKph { get; set; }

    /// <summary>
    /// Heading in degrees.
    /// Note: our domain treats heading as a half-open range [0, 360).
    /// </summary>
    [Range(0, 360)]
    public double? HeadingDegrees { get; set; }

    /// <summary>
    /// Estimated horizontal accuracy in meters.
    /// </summary>
    [Range(0, double.MaxValue)]
    public double? AccuracyMeters { get; set; }

    /// <summary>
    /// Altitude in meters.
    /// </summary>
    public double? AltitudeMeters { get; set; }

    /// <summary>
    /// Odometer reading in kilometres.
    /// </summary>
    public double? OdometerKm { get; set; }

    /// <summary>
    /// Client-side correlation identifier for traceability/idempotency purposes.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string CorrelationId { get; set; } = null!;
}
