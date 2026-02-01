using System.Text.Json.Serialization;

namespace GeoTrack.API.Contracts.Vehicles;

/// <summary>
/// Paged list response.
/// </summary>
public sealed class PagedVehiclesResponse
{
    // Page items.
    [JsonPropertyName("items")]
    public IReadOnlyList<VehicleResponse> Items { get; set; } = Array.Empty<VehicleResponse>();

    // 1-based page number.
    [JsonPropertyName("page")]
    public int Page { get; set; }

    // Page size (number of items requested).
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    // Total number of vehicles available for the current tenant.
    [JsonPropertyName("total")]
    public int Total { get; set; }
}
