using GeoTrack.API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GeoTrack.API.Specs.Infrastructure;

public sealed class GeoTrackApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove ANY existing registrations for the application's DbContext so we don't end up with
            // both Npgsql + InMemory providers in the same service provider.
            RemoveAll(services, d => d.ServiceType == typeof(TrackingDbContext));
            RemoveAll(services, d => d.ServiceType == typeof(DbContextOptions<TrackingDbContext>));
            RemoveAll(services, d => d.ServiceType == typeof(DbContextOptions));

            // Remove any registered EF Core provider services (Npgsql, etc.)
            RemoveAll(services, d => d.ServiceType == typeof(Microsoft.EntityFrameworkCore.Storage.IDatabaseProvider));

            // Unique DB per factory/run
            var dbName = $"GeoTrack_TestDb_{Guid.NewGuid():N}";
            services.AddDbContext<TrackingDbContext>(o => o.UseInMemoryDatabase(dbName));
        });
    }

    private static void RemoveAll(IServiceCollection services, Func<ServiceDescriptor, bool> predicate)
    {
        var matches = services.Where(predicate).ToList();
        foreach (var d in matches)
            services.Remove(d);
    }
}
