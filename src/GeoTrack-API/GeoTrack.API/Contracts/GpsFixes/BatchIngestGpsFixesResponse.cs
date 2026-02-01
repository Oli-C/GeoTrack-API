using System;
using System.Collections.Generic;

namespace GeoTrack.API.Contracts.GpsFixes;

/// <summary>
/// Response body for batch GPS fix ingestion.
/// </summary>
public sealed class BatchIngestGpsFixesResponse
{
    /// <summary>
    /// Number of items accepted and persisted.
    /// </summary>
    public required int AcceptedCount { get; init; }

    /// <summary>
    /// Number of items rejected (not persisted).
    /// </summary>
    public required int RejectedCount { get; init; }

    /// <summary>
    /// Server timestamp (UTC) when the batch was received.
    /// </summary>
    public required DateTime ReceivedAtUtc { get; init; }

    /// <summary>
    /// Per-item results aligned with the original request order.
    /// </summary>
    public required List<ItemResult> Results { get; init; }

    /// <summary>
    /// Result for an individual item within a batch.
    /// </summary>
    public sealed class ItemResult
    {
        /// <summary>
        /// Zero-based index of the item in the request <c>Items</c> list.
        /// </summary>
        public required int Index { get; init; }

        /// <summary>
        /// The vehicle ID from the request item.
        /// </summary>
        public required Guid VehicleId { get; init; }

        /// <summary>
        /// Either <c>accepted</c> or <c>rejected</c>.
        /// </summary>
        public required string Status { get; init; }

        /// <summary>
        /// The persisted GPS fix id when <see cref="Status"/> is <c>accepted</c>.
        /// </summary>
        public Guid? GpsFixId { get; init; }

        /// <summary>
        /// Stable machine-readable rejection code when <see cref="Status"/> is <c>rejected</c>.
        /// </summary>
        public string? Error { get; init; }

        /// <summary>
        /// Human-readable explanation when <see cref="Status"/> is <c>rejected</c>.
        /// </summary>
        public string? Message { get; init; }
    }
}
