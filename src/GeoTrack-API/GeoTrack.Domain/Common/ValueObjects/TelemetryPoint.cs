using System;

namespace GeoTrack.Domain.Common.ValueObjects
{
    public abstract class TelemetryPoint : ITelemetryPoint
    {
        public Guid Id { get; protected set; }
        public Guid TenantId { get; protected set; }
        public Guid VehicleId { get; protected set; }

        public TelemetrySource Source { get; protected set; }

        public DateTime DeviceTimeUtc { get; protected set; }
        public DateTime ReceivedAtUtc { get; protected set; }

        // helps ordering when device timestamps are unreliable
        public long? DeviceSequence { get; protected set; }

        // Optional: raw payload traceability (reference types are nullable by default in .NET Standard 2
        public string CorrelationId { get; protected set; }

        protected TelemetryPoint()
        {
            // EF Core needs parameterless ctor.
            // Default Source here instead of property initializer (keeps older compiler happy too).
            Source = TelemetrySource.Device;
        }
    }
}