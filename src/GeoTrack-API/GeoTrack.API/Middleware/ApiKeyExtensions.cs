using Microsoft.Extensions.Options;

namespace GeoTrack.API.Middleware;

public static class ApiKeyExtensions
{
    public static IServiceCollection AddApiKeyAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<ApiKeyOptions>()
            .Bind(configuration.GetSection(ApiKeyOptions.SectionName))
            .Validate(o => !o.Enabled || (o.Keys is { Length: > 0 }), "ApiKey:Keys must be configured when ApiKey:Enabled is true.")
            .ValidateOnStart();

        services.AddTransient<ApiKeyMiddleware>();

        return services;
    }

    public static IApplicationBuilder UseApiKeyAuth(this IApplicationBuilder app)
    {
        // Middleware is registered as IMiddleware, so we use UseMiddleware<T>() and let DI instantiate it.
        return app.UseMiddleware<ApiKeyMiddleware>();
    }
}
