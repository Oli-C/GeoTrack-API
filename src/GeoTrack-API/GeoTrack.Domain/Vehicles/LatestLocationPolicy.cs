using System;
using GeoTrack.Domain.Common.ValueObjects;

namespace GeoTrack.Domain.Vehicles
{
    /// <summary>
    /// Domain policy for deciding whether a candidate GPS fix should replace a vehicle's current latest location.
    /// </summary>
    public static class LatestLocationPolicy
    {
        /// <summary>
        /// Returns true if <paramref name="candidate"/> should replace <paramref name="current"/>.
        /// Ordering: DeviceTimeUtc wins; ties broken by DeviceSequence (null treated as 0).
        /// </summary>
        public static bool ShouldReplace(VehicleLatestLocation current, GpsFix candidate)
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));

            if (current == null)
                return true;

            if (candidate.DeviceTimeUtc > current.DeviceTimeUtc) return true;
            if (candidate.DeviceTimeUtc < current.DeviceTimeUtc) return false;

            var candSeq = candidate.DeviceSequence ?? 0;
            return candSeq > current.DeviceSequence;
        }

        /// <summary>
        /// Returns true if <paramref name="candidate"/> is newer than <paramref name="current"/>.
        /// Ordering: DeviceTimeUtc wins; ties broken by DeviceSequence (null treated as 0).
        /// </summary>
        public static bool IsNewer(GpsFix candidate, GpsFix current)
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));
            if (current == null) throw new ArgumentNullException(nameof(current));

            if (candidate.DeviceTimeUtc > current.DeviceTimeUtc) return true;
            if (candidate.DeviceTimeUtc < current.DeviceTimeUtc) return false;

            var candSeq = candidate.DeviceSequence ?? 0;
            var currSeq = current.DeviceSequence ?? 0;
            return candSeq > currSeq;
        }

        /// <summary>
        /// Creates a new latest location snapshot from a GPS fix.
        /// </summary>
        public static VehicleLatestLocation CreateSnapshot(
            Guid tenantId,
            Guid vehicleId,
            GpsFix gpsFix,
            Guid? routeScheduleId,
            DateTime updatedAtUtc)
        {
            if (gpsFix == null) throw new ArgumentNullException(nameof(gpsFix));
            if (updatedAtUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException("updatedAtUtc must be UTC.", nameof(updatedAtUtc));

            var sequence = gpsFix.DeviceSequence ?? 0;

            return new VehicleLatestLocation(
                tenantId: tenantId,
                vehicleId: vehicleId,
                gpsFixId: gpsFix.Id,
                deviceTimeUtc: gpsFix.DeviceTimeUtc,
                receivedAtUtc: gpsFix.ReceivedAtUtc,
                deviceSequence: sequence,
                latitude: gpsFix.Latitude.Value,
                longitude: gpsFix.Longitude.Value,
                speedKph: gpsFix.Speed?.Value,
                headingDegrees: gpsFix.Heading?.Value,
                accuracyMeters: gpsFix.Accuracy?.Value,
                routeScheduleId: routeScheduleId,
                updatedAtUtc: updatedAtUtc);
        }
    }
}
