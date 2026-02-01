using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using GeoTrack.API.Data.Entities;

namespace GeoTrack.API.Contracts.Vehicles;

/// <summary>
/// Payload used to partially update a vehicle.
/// </summary>
/// <remarks>
/// Only properties present in the JSON payload are applied.
/// For the string fields, sending an explicit <c>null</c> will clear the value.
/// </remarks>
public sealed class PatchVehicleRequest
{
    /// <summary>
    /// Updates the registration/plate identifier.
    /// Must be 32 characters or fewer.
    /// </summary>
    [StringLength(32)]
    [JsonPropertyName("registrationNumber")]
    public string? RegistrationNumber { get; set; }

    /// <summary>
    /// Updates the friendly display name.
    /// Must be 128 characters or fewer.
    /// </summary>
    [StringLength(128)]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Updates the external identifier.
    /// Must be 128 characters or fewer.
    /// </summary>
    [StringLength(128)]
    [JsonPropertyName("externalId")]
    public string? ExternalId { get; set; }

    /// <summary>
    /// Updates the lifecycle status.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description><see cref="VehicleStatus.Active"/>: in service and trackable.</description></item>
    /// <item><description><see cref="VehicleStatus.Inactive"/>: temporarily out of service.</description></item>
    /// <item><description><see cref="VehicleStatus.Decommissioned"/>: permanently retired (cannot be re-activated).</description></item>
    /// </list>
    /// </remarks>
    [EnumDataType(typeof(VehicleStatus))]
    [JsonPropertyName("status")]
    public VehicleStatus? Status { get; set; }
}
