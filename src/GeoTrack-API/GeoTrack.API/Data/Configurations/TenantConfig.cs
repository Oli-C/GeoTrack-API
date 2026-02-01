using GeoTrack.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeoTrack.API.Data.Configurations;

public sealed class TenantConfig : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.ToTable("tenants");

        b.HasKey(t => t.Id);

        b.Property(t => t.Id)
            .HasColumnName("id");

        b.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(128)
            .IsRequired();

        b.Property(t => t.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()")
            .IsRequired();

        b.HasIndex(t => t.Name)
            .IsUnique()
            .HasDatabaseName("ux_tenants_name");
    }
}
