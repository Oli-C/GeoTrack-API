using System;
using GeoTrack.API.Data.Entities;

namespace GeoTrack.Domain.Vehicles
{
    /// <summary>
    /// A snapshot of the latest known location for a vehicle.
    /// One row per (tenant_id, vehicle_id).
    /// </summary>
    public sealed class VehicleLatestLocation
    {
        private VehicleLatestLocation() { } // EF

        public VehicleLatestLocation(
            Guid tenantId,
            Guid vehicleId,
            Guid gpsFixId,
            DateTime deviceTimeUtc,
            DateTime receivedAtUtc,
            long deviceSequence,
            double latitude,
            double longitude,
            double? speedKph = null,
            double? headingDegrees = null,
            double? accuracyMeters = null,
            Guid? routeScheduleId = null,
            DateTime? updatedAtUtc = null)
        {
            if (tenantId == Guid.Empty) throw new ArgumentException("TenantId is required.", nameof(tenantId));
            if (vehicleId == Guid.Empty) throw new ArgumentException("VehicleId is required.", nameof(vehicleId));
            if (gpsFixId == Guid.Empty) throw new ArgumentException("GpsFixId is required.", nameof(gpsFixId));
            if (deviceTimeUtc.Kind != DateTimeKind.Utc) throw new ArgumentException("deviceTimeUtc must be UTC.", nameof(deviceTimeUtc));
            if (receivedAtUtc.Kind != DateTimeKind.Utc) throw new ArgumentException("receivedAtUtc must be UTC.", nameof(receivedAtUtc));

            ValidateLatitude(latitude);
            ValidateLongitude(longitude);
            ValidateOptionalNonNegative(speedKph, nameof(speedKph));
            ValidateOptionalHeading(headingDegrees, nameof(headingDegrees));
            ValidateOptionalNonNegative(accuracyMeters, nameof(accuracyMeters));

            TenantId = tenantId;
            VehicleId = vehicleId;
            GpsFixId = gpsFixId;
            DeviceTimeUtc = deviceTimeUtc;
            ReceivedAtUtc = receivedAtUtc;
            DeviceSequence = deviceSequence;
            Latitude = latitude;
            Longitude = longitude;
            SpeedKph = speedKph;
            HeadingDegrees = headingDegrees;
            AccuracyMeters = accuracyMeters;
            RouteScheduleId = routeScheduleId;
            UpdatedAtUtc = updatedAtUtc ?? DateTime.UtcNow;
        }

        public Guid TenantId { get; private set; }
        public Guid VehicleId { get; private set; }

        public Guid GpsFixId { get; private set; }

        public DateTime DeviceTimeUtc { get; private set; }
        public DateTime ReceivedAtUtc { get; private set; }
        public long DeviceSequence { get; private set; }

        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        public double? SpeedKph { get; private set; }
        public double? HeadingDegrees { get; private set; }
        public double? AccuracyMeters { get; private set; }

        public Guid? RouteScheduleId { get; private set; }

        public DateTime UpdatedAtUtc { get; private set; }

        // Optional navigation
        public Vehicle Vehicle { get; private set; }

        private static void ValidateLatitude(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < -90 || value > 90)
                throw new ArgumentOutOfRangeException(nameof(value), "Latitude must be between -90 and +90.");
        }

        private static void ValidateLongitude(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < -180 || value > 180)
                throw new ArgumentOutOfRangeException(nameof(value), "Longitude must be between -180 and +180.");
        }

        private static void ValidateOptionalNonNegative(double? value, string name)
        {
            if (!value.HasValue) return;
            if (double.IsNaN(value.Value) || double.IsInfinity(value.Value) || value.Value < 0)
                throw new ArgumentOutOfRangeException(name, name + " must be >= 0.");
        }

        private static void ValidateOptionalHeading(double? value, string name)
        {
            if (!value.HasValue) return;

            var v = value.Value;
            if (double.IsNaN(v) || double.IsInfinity(v) || v < 0 || v >= 360)
                throw new ArgumentOutOfRangeException(name, name + " must be in range [0, 360).");
        }
    }
}
