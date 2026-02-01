using GeoTrack.API.Common;
using GeoTrack.API.Data;
using GeoTrack.API.Middleware;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// Services
// ----------------------------
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddApiKeyAuth(builder.Configuration);

builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<TenantResolutionMiddleware>();

// Connection string (Postgres)
var postgresConnectionString =
    builder.Configuration.GetConnectionString("Postgres")
    ?? builder.Configuration["ConnectionStrings:Postgres"];

if (!builder.Environment.IsEnvironment("Testing"))
{
    if (string.IsNullOrWhiteSpace(postgresConnectionString))
    {
        throw new InvalidOperationException(
            "Missing connection string 'Postgres'. Set ConnectionStrings:Postgres in appsettings.json or environment variables.");
    }

    builder.Services.AddDbContext<TrackingDbContext>(options =>
    {
        options.UseNpgsql(postgresConnectionString, npgsql =>
        {
            // Keep migrations in the default assembly migrations folder
            // (GeoTrack.API/Migrations). Avoid pointing at any legacy migration namespaces.
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory");
        });

        // Helpful during development
        if (builder.Environment.IsDevelopment())
        {
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
        }
    });
}

var app = builder.Build();

// ----------------------------
// Database initialization
// ----------------------------
// Auto-apply EF migrations on startup (for take-home / container startup convenience).
// Includes a small retry loop to avoid flaking during cold starts.
if (!app.Environment.IsEnvironment("Testing"))
{
    const int maxAttempts = 10;
    var delay = TimeSpan.FromSeconds(1);

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TrackingDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupMigrations");

    Exception? lastError = null;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await db.Database.MigrateAsync();
            lastError = null;
            break;
        }
        catch (Exception ex)
        {
            lastError = ex;
            logger.LogWarning(ex, "Database migration attempt {Attempt}/{MaxAttempts} failed. Retrying in {Delay}...", attempt, maxAttempts, delay);
            await Task.Delay(delay);
            delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, 8000));
        }
    }

    if (lastError is not null)
    {
        throw new InvalidOperationException(
            $"Failed to apply database migrations after {maxAttempts} attempts. See inner exception for details.",
            lastError);
    }
}

// ----------------------------
// HTTP pipeline
// ----------------------------
if (app.Environment.IsDevelopment())
{
    // Public docs in Development only.
    // These endpoints are mapped before the API key middleware runs, so they won't require a key.
    app.MapOpenApi().AllowAnonymous();

    // Scalar UI for the OpenAPI document
    // Default route: /scalar/v1
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseHttpsRedirection();

app.UseApiKeyAuth();
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
