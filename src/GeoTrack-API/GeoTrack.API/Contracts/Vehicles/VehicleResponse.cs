using System.Text.Json.Serialization;
using GeoTrack.API.Data.Entities;

namespace GeoTrack.API.Contracts.Vehicles;

/// <summary>
/// Vehicle representation returned by the API.
/// </summary>
public sealed class VehicleResponse
{
    /// <summary>
    /// Vehicle identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

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
    /// External identifier from an upstream system (if set).
    /// </summary>
    [JsonPropertyName("externalId")]
    public string? ExternalId { get; set; }

    /// <summary>
    /// When the vehicle was created (UTC).
    /// </summary>
    [JsonPropertyName("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Current lifecycle status.
    /// </summary>
    [JsonPropertyName("status")]
    public VehicleStatus Status { get; set; }
}
