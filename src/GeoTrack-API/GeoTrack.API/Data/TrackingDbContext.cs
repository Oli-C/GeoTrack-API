using GeoTrack.API.Data.Entities;
using GeoTrack.Domain.Common.ValueObjects;
using GeoTrack.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace GeoTrack.API.Data;

public sealed class TrackingDbContext : DbContext
{
    public TrackingDbContext(DbContextOptions<TrackingDbContext> options)
        : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<GpsFix> GpsFixes => Set<GpsFix>();
    public DbSet<VehicleLatestLocation> VehicleLatestLocations => Set<VehicleLatestLocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TrackingDbContext).Assembly);
    }
}