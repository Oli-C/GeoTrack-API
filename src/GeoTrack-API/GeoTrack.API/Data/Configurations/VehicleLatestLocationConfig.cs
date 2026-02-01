using GeoTrack.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeoTrack.API.Data.Configurations;

public sealed class VehicleLatestLocationConfig : IEntityTypeConfiguration<VehicleLatestLocation>
{
    public void Configure(EntityTypeBuilder<VehicleLatestLocation> b)
    {
        b.ToTable("vehicle_latest_location");

        b.Property(x => x.TenantId)
            .HasColumnName("tenant_id");

        b.Property(x => x.VehicleId)
            .HasColumnName("vehicle_id");

        b.HasKey(x => new { x.TenantId, x.VehicleId });

        b.Property(x => x.GpsFixId)
            .HasColumnName("gps_fix_id");

        b.Property(x => x.DeviceTimeUtc)
            .HasColumnName("device_time_utc");

        b.Property(x => x.ReceivedAtUtc)
            .HasColumnName("received_at_utc");

        b.Property(x => x.DeviceSequence)
            .HasColumnName("device_sequence");

        b.Property(x => x.Latitude)
            .HasColumnName("latitude");

        b.Property(x => x.Longitude)
            .HasColumnName("longitude");

        b.Property(x => x.SpeedKph)
            .HasColumnName("speed_kph");

        b.Property(x => x.HeadingDegrees)
            .HasColumnName("heading_degrees");

        b.Property(x => x.AccuracyMeters)
            .HasColumnName("accuracy_meters");

        b.Property(x => x.RouteScheduleId)
            .HasColumnName("route_schedule_id");

        b.Property(x => x.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasDefaultValueSql("now()");

        b.HasOne(x => x.Vehicle)
            .WithOne()
            .HasForeignKey<VehicleLatestLocation>(x => new { x.TenantId, x.VehicleId })
            .HasPrincipalKey<GeoTrack.API.Data.Entities.Vehicle>(v => new { v.TenantId, v.Id })
            .OnDelete(DeleteBehavior.Cascade);

        // Fast reads
        b.HasIndex(x => new { x.TenantId, x.VehicleId })
            .HasDatabaseName("ix_vll_tenant_vehicle");

        // Optional query patterns
        b.HasIndex(x => new { x.TenantId, x.ReceivedAtUtc })
            .HasDatabaseName("ix_vll_tenant_receivedat");
    }
}
