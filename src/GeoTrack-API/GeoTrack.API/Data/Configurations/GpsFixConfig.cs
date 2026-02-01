using GeoTrack.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeoTrack.API.Data.Configurations;

public sealed class GpsFixConfig : IEntityTypeConfiguration<GpsFix>
{
    public void Configure(EntityTypeBuilder<GpsFix> b)
    {
        b.ToTable("gps_fixes");

        b.HasKey(x => x.Id);

        b.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        b.Property(x => x.VehicleId)
            .HasColumnName("vehicle_id")
            .IsRequired();

        b.HasOne<GeoTrack.API.Data.Entities.Vehicle>()
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.VehicleId })
            .HasPrincipalKey(v => new { v.TenantId, v.Id })
            .OnDelete(DeleteBehavior.Cascade);

        // TelemetryPoint base
        b.Property(x => x.DeviceTimeUtc)
            .HasColumnName("device_time_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        b.Property(x => x.ReceivedAtUtc)
            .HasColumnName("received_at_utc")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()")
            .IsRequired();

        b.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(128)
            .IsRequired();

        b.Property(x => x.DeviceSequence)
            .HasColumnName("device_sequence");

        // Enums
        b.Property(x => x.Source)
            .HasColumnName("source")
            .HasConversion<int>()
            .IsRequired();

        b.Property(x => x.Quality)
            .HasColumnName("quality")
            .HasConversion<int>()
            .IsRequired();

        // Latitude/Longitude (value objects)
        b.Property(x => x.Latitude)
            .HasColumnName("latitude")
            .HasPrecision(9, 6)
            .HasConversion(
                v => v.Value,
                v => Latitude.From(v))
            .IsRequired();

        b.Property(x => x.Longitude)
            .HasColumnName("longitude")
            .HasPrecision(9, 6)
            .HasConversion(
                v => v.Value,
                v => Longitude.From(v))
            .IsRequired();

        // Optional value objects
        b.Property(x => x.Speed)
            .HasColumnName("speed_kph")
            .HasPrecision(9, 3)
            .HasConversion(
                v => v.HasValue ? v.Value.Value : (double?)null,
                v => SpeedKph.FromNullable(v));

        b.Property(x => x.Heading)
            .HasColumnName("heading_degrees")
            .HasPrecision(9, 3)
            .HasConversion(
                v => v.HasValue ? v.Value.Value : (double?)null,
                v => HeadingDegrees.FromNullable(v));

        b.Property(x => x.Accuracy)
            .HasColumnName("accuracy_meters")
            .HasPrecision(9, 3)
            .HasConversion(
                v => v.HasValue ? v.Value.Value : (double?)null,
                v => AccuracyMeters.FromNullable(v));

        // Plain primitives
        b.Property(x => x.AltitudeMeters)
            .HasColumnName("altitude_meters")
            .HasPrecision(9, 3)
            .HasConversion(
                v => v.HasValue ? (double?)v.Value.Value : null,
                v => AltitudeMeters.FromNullable(v));

        b.Property(x => x.OdometerKm)
            .HasColumnName("odometer_km")
            .HasPrecision(14, 3)
            .HasConversion(
                v => v.HasValue ? (double?)v.Value.Value : null,
                v => OdometerKm.FromNullable(v));

        // Useful indexes
        b.HasIndex(x => new { x.TenantId, x.VehicleId, x.DeviceTimeUtc })
            .HasDatabaseName("ix_gpsfix_tenant_vehicle_devicetime");

        b.HasIndex(x => x.ReceivedAtUtc)
            .HasDatabaseName("ix_gpsfix_receivedat");
    }
}
