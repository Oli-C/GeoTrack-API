using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GeoTrack.API.Contracts.Vehicles;

/// <summary>
/// Payload used to create a vehicle for the current tenant.
/// </summary>
public sealed class CreateVehicleRequest
{
    // Optional registration/plate identifier.
    // Must be 32 characters or fewer.
    [StringLength(32)]
    [JsonPropertyName("registrationNumber")]
    public string? RegistrationNumber { get; set; }

    // Optional friendly display name.
    // Must be 128 characters or fewer.
    [StringLength(128)]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    // Optional external identifier (e.g. from an upstream fleet system).
    // Must be 128 characters or fewer.
    [StringLength(128)]
    [JsonPropertyName("externalId")]
    public string? ExternalId { get; set; }
}
