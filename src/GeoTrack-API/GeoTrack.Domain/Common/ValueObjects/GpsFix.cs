using System;

namespace GeoTrack.Domain.Common.ValueObjects
{
    public sealed class GpsFix : TelemetryPoint, IHasGeoLocation
    {
        public Latitude Latitude { get; private set; }
        public Longitude Longitude { get; private set; }

        public SpeedKph? Speed { get; private set; }
        public HeadingDegrees? Heading { get; private set; }
        public AccuracyMeters? Accuracy { get; private set; }

        public AltitudeMeters? AltitudeMeters { get; private set; }
        public OdometerKm? OdometerKm { get; private set; }

        public FixQuality Quality { get; private set; }

        private GpsFix()
        {
            // EF Core
            Quality = FixQuality.Unknown;
        }

        public GpsFix(
            Guid id,
            Guid tenantId,
            Guid vehicleId,
            Latitude latitude,
            Longitude longitude,
            DateTime deviceTimeUtc,
            DateTime receivedAtUtc,
            TelemetrySource source,
            DeviceSequence? deviceSequence,
            SpeedKph? speed,
            HeadingDegrees? heading,
            AccuracyMeters? accuracy,
            AltitudeMeters? altitudeMeters,
            OdometerKm? odometerKm,
            CorrelationId correlationId,
            FixQuality? qualityOverride = null)
        {
            if (id == Guid.Empty) throw new ArgumentException("Id is required.", nameof(id));
            // TenantId is enforced by API/middleware + DB constraints. Allow Guid.Empty here to keep
            // EF materialization and legacy call sites working.
            if (vehicleId == Guid.Empty) throw new ArgumentException("VehicleId is required.", nameof(vehicleId));
            if (deviceTimeUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException("deviceTimeUtc must be UTC.", nameof(deviceTimeUtc));
            if (receivedAtUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException("receivedAtUtc must be UTC.", nameof(receivedAtUtc));

            Id = id;
            TenantId = tenantId;
            VehicleId = vehicleId;

            Latitude = latitude;
            Longitude = longitude;

            DeviceTimeUtc = deviceTimeUtc;
            ReceivedAtUtc = receivedAtUtc;
            Source = source;

            DeviceSequence = deviceSequence.HasValue ? (long?)deviceSequence.Value.Value : null;

            Speed = speed;
            Heading = heading;
            Accuracy = accuracy;
            AltitudeMeters = altitudeMeters;
            OdometerKm = odometerKm;
            Quality = qualityOverride ?? FixQuality.Unknown;

            CorrelationId = correlationId.Value;
        }

        public GpsFix(
            Guid id,
            Guid vehicleId,
            Latitude latitude,
            Longitude longitude,
            DateTime deviceTimeUtc,
            DateTime receivedAtUtc,
            TelemetrySource source,
            DeviceSequence? deviceSequence,
            SpeedKph? speed,
            HeadingDegrees? heading,
            AccuracyMeters? accuracy,
            AltitudeMeters? altitudeMeters,
            OdometerKm? odometerKm,
            CorrelationId correlationId)
            : this(
                id: id,
                tenantId: Guid.Empty,
                vehicleId: vehicleId,
                latitude: latitude,
                longitude: longitude,
                deviceTimeUtc: deviceTimeUtc,
                receivedAtUtc: receivedAtUtc,
                source: source,
                deviceSequence: deviceSequence,
                speed: speed,
                heading: heading,
                accuracy: accuracy,
                altitudeMeters: altitudeMeters,
                odometerKm: odometerKm,
                correlationId: correlationId)
        {
        }

        public GpsFix(
            Guid id,
            Guid vehicleId,
            Latitude latitude,
            Longitude longitude,
            DateTime deviceTimeUtc,
            DateTime receivedAtUtc,
            TelemetrySource source,
            DeviceSequence? deviceSequence,
            SpeedKph? speed,
            HeadingDegrees? heading,
            AccuracyMeters? accuracy,
            AltitudeMeters? altitudeMeters,
            OdometerKm? odometerKm,
            FixQuality quality,
            CorrelationId correlationId)
            : this(
                id: id,
                tenantId: Guid.Empty,
                vehicleId: vehicleId,
                latitude: latitude,
                longitude: longitude,
                deviceTimeUtc: deviceTimeUtc,
                receivedAtUtc: receivedAtUtc,
                source: source,
                deviceSequence: deviceSequence,
                speed: speed,
                heading: heading,
                accuracy: accuracy,
                altitudeMeters: altitudeMeters,
                odometerKm: odometerKm,
                correlationId: correlationId,
                qualityOverride: quality)
        {
        }
    }
}