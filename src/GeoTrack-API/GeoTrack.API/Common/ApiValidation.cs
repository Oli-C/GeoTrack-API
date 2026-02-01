namespace GeoTrack.API.Common;

/// <summary>
/// Shared validation rules (limits + messages) used across API endpoints.
/// Keep these consistent to avoid drifting validation behaviour between controllers.
/// </summary>
public static class ApiValidation
{
    public static class Paging
    {
        public const int DefaultPage = 1;
        public const int DefaultPageSize = 50;

        public const int MinPage = 1;
        public const int MinPageSize = 1;
        public const int MaxPageSize = 200;

        public const string PageMessage = "page must be >= 1";
        public const string PageSizeMessage = "pageSize must be between 1 and 200";
    }

    public static class VehicleProgress
    {
        public const int DefaultWindowMinutes = 60;
        public const int MinWindowMinutes = 1;
        public const int MaxWindowMinutes = 24 * 60;
        public const string WindowMinutesMessage = "windowMinutes must be between 1 and 1440";

        public const int DefaultStaleAfterSeconds = 300;
        public const int MinStaleAfterSeconds = 1;
        public const int MaxStaleAfterSeconds = 24 * 60 * 60;
        public const string StaleAfterSecondsMessage = "staleAfterSeconds must be between 1 and 86400";
    }

    public static class GpsFixes
    {
        public const int HeadingDegreesMaxExclusive = 360;

        public const string DeviceTimeUtcMustBeUtcMessage = "deviceTimeUtc must be UTC";
        public const string HeadingDegreesRangeMessage = "headingDegrees must be in range [0, 360)";
    }
}
