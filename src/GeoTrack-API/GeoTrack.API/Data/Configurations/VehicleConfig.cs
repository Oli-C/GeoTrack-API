using GeoTrack.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeoTrack.API.Data.Configurations;

public sealed class VehicleConfig : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> b)
    {
        b.ToTable("vehicles");

        b.Property(v => v.TenantId)
            .HasColumnName("tenant_id");

        b.Property(v => v.Id)
            .HasColumnName("id");

        b.HasKey(v => new { v.TenantId, v.Id });

        b.Property(v => v.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasDefaultValueSql("now()");

        // Optimistic concurrency (PostgreSQL)
        b.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        // Latest pointer (optional)
        // Removed: latest_location_progress_id is replaced by vehicle_latest_location snapshot table.

        // Status
        b.Property(v => v.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .HasDefaultValue(VehicleStatus.Active);

        // Relationship to latest progress uses composite FK via LP TenantId + Id.
        // Removed.

        // History removed: vehicle_location_progress table is no longer used.

        // Owned Identity value object
        b.OwnsOne(v => v.Identity, id =>
        {
            id.Property(p => p.RegistrationNumber)
                .HasColumnName("registration_number")
                .HasMaxLength(32);

            id.Property(p => p.Name)
                .HasColumnName("name")
                .HasMaxLength(128);

            id.Property(p => p.ExternalId)
                .HasColumnName("external_id")
                .HasMaxLength(128);

            // Unique per-tenant when present (Postgres partial index)
            // Note: EF creates shadow FK properties for the owned type: VehicleTenantId + VehicleId.
            id.HasIndex("VehicleTenantId", nameof(VehicleIdentity.RegistrationNumber))
                .IsUnique()
                .HasDatabaseName("ux_vehicle_tenant_registration_number")
                .HasFilter("registration_number IS NOT NULL");
        });

        b.Navigation(v => v.Identity).IsRequired();

        // Helpful index for tenant scoping
        b.HasIndex(v => v.TenantId)
            .HasDatabaseName("ix_vehicles_tenant");
    }
}