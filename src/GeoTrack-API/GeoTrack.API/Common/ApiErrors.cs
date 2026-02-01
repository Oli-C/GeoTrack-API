namespace GeoTrack.API.Common;

/// <summary>
/// Central place for stable API error codes (and their default messages).
/// Prefer using the <c>*Code</c> values as contract-stable identifiers.
/// </summary>
public static class ApiErrors
{
    public static class Tenant
    {
        public const string MissingCode = "missing_tenant";
        public const string InvalidCode = "invalid_tenant";

        public const string MissingMessage = "Missing required tenant header.";
        public const string InvalidMessage = "Tenant header must be a non-empty GUID.";
    }

    public static class ApiKey
    {
        public const string NotConfiguredCode = "api_key_not_configured";
        public const string MissingCode = "missing_api_key";
        public const string InvalidCode = "invalid_api_key";

        public const string NotConfiguredMessage = "API key authentication is enabled but no keys are configured.";
        public const string MissingMessage = "Missing required API key header.";
        public const string InvalidMessage = "The provided API key is invalid.";
    }

    public static class Vehicles
    {
        public const string DuplicateRegistrationNumberCode = "duplicate_registration_number";
        public const string DuplicateRegistrationNumberMessage = "A vehicle with the same registration number already exists for this tenant.";
    }

    public static class GpsFixes
    {
        // Batch ingest item statuses
        public const string AcceptedStatus = "accepted";
        public const string RejectedStatus = "rejected";

        // Batch ingest item errors
        public const string InvalidVehicleCode = "invalid_vehicle";
        public const string InvalidVehicleMessage = "vehicleId must be a non-empty GUID";

        public const string VehicleNotFoundCode = "vehicle_not_found";
        public const string VehicleNotFoundMessage = "Vehicle not found for tenant";

        public const string InvalidDeviceTimeCode = "invalid_device_time";
        public const string InvalidDeviceTimeMessage = "deviceTimeUtc must be UTC";

        public const string InvalidHeadingCode = "invalid_heading";
        public const string InvalidHeadingMessage = "headingDegrees must be in range [0, 360)";

        public const string MissingCorrelationIdCode = "missing_correlation_id";
        public const string MissingCorrelationIdMessage = "correlationId is required";

        public const string InvalidPayloadCode = "invalid_payload";
    }

    public static class Validation
    {
        public const string InvalidQueryCode = "invalid_query";
        public const string InvalidPagingCode = "invalid_paging";
    }
}
