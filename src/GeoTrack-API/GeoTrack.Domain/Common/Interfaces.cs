using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeoTrack.API.Data.Entities;
using GeoTrack.Domain.Common.ValueObjects;

namespace GeoTrack.Domain.Common
{
    public interface ITelemetryPoint
    {
        Guid Id { get; }
        Guid VehicleId { get; }
        TelemetrySource Source { get; }
        DateTime DeviceTimeUtc { get; }
        DateTime ReceivedAtUtc { get; }
    }

    public interface IHasGeoLocation
    {
        Latitude Latitude { get; }
        Longitude Longitude { get; }
    }

    public interface IHasSpeed
    {
        SpeedKph Speed { get; }
    }

    public interface IHasHeading
    {
        HeadingDegrees Heading { get; }
    }

    public interface IHasAccuracy
    {
        AccuracyMeters Accuracy { get; }
    }

    public interface IClock
    {
        DateTime UtcNow { get; }
    }

    public interface IVehicleRepository
    {
        Task<Vehicle> GetAsync(Guid vehicleId, CancellationToken ct);
        Task AddAsync(Vehicle vehicle, CancellationToken ct);
    }

    public interface IGpsFixRepository
    {
        Task AddAsync(GpsFix fix, CancellationToken ct);

        Task<GpsFix> GetLatestAsync(Guid vehicleId, CancellationToken ct);

        Task<IReadOnlyList<GpsFix>> GetRangeAsync(
            Guid vehicleId,
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken ct);
    }

    public interface ILatestFixPolicy
    {
        bool IsNewer(GpsFix candidate, GpsFix currentLatest);
    }

    // Default: newest by DeviceTimeUtc, tie-break by sequence, then ReceivedAtUtc
    public sealed class LatestByDeviceTimePolicy : ILatestFixPolicy
    {
        public bool IsNewer(GpsFix c, GpsFix cur)
        {
            if (c.DeviceTimeUtc != cur.DeviceTimeUtc) return c.DeviceTimeUtc > cur.DeviceTimeUtc;
            if (c.DeviceSequence.HasValue && cur.DeviceSequence.HasValue && c.DeviceSequence != cur.DeviceSequence)
                return c.DeviceSequence > cur.DeviceSequence;
            return c.ReceivedAtUtc > cur.ReceivedAtUtc;
        }
    }

    public interface IDistanceCalculator
    {
        double KmBetween(IHasGeoLocation a, IHasGeoLocation b);
    }

}