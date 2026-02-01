using System;

namespace GeoTrack.API.Contracts.Vehicles;

/// <summary>
/// Response body for single GPS fix ingestion.
/// </summary>
public sealed class IngestGpsFixResponse
{
    /// <summary>
    /// Vehicle the fix was ingested for.
    /// </summary>
    public required Guid VehicleId { get; init; }

    /// <summary>
    /// ID assigned to the persisted GPS fix.
    /// </summary>
    public required Guid GpsFixId { get; init; }

    /// <summary>
    /// Device timestamp (UTC) echoed back from the request.
    /// </summary>
    public required DateTime DeviceTimeUtc { get; init; }

    /// <summary>
    /// Server timestamp (UTC) when the fix was received.
    /// </summary>
    public required DateTime ReceivedAtUtc { get; init; }

    /// <summary>
    /// True when this fix became the vehicle's latest location snapshot.
    /// </summary>
    public required bool IsLatestApplied { get; init; }
}
