using Microsoft.Extensions.Options;
using GeoTrack.API.Common;

namespace GeoTrack.API.Middleware;

public sealed class ApiKeyMiddleware : IMiddleware
{
    private readonly ApiKeyOptions _options;
    private readonly IHostEnvironment _environment;

    public ApiKeyMiddleware(IOptions<ApiKeyOptions> options, IHostEnvironment environment)
    {
        _options = options.Value;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!_options.Enabled)
        {
            await next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;

        // Allow docs to be viewed without an API key in Development only.
        // Prefix match so /openapi/v1.json and /scalar/v1 (etc.) are covered.
        if (_environment.IsDevelopment() &&
            (path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase) ||
             path.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        // Allow simple anonymous endpoints (e.g., health checks)
        if (_options.AllowAnonymousPaths.Any(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        // If no keys are configured, fail closed.
        if (_options.Keys is not { Length: > 0 })
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new
            {
                error = ApiErrors.ApiKey.NotConfiguredCode,
                message = ApiErrors.ApiKey.NotConfiguredMessage
            });
            return;
        }

        var headerName = string.IsNullOrWhiteSpace(_options.HeaderName) ? ApiHeaders.ApiKeyDefault : _options.HeaderName;

        if (!context.Request.Headers.TryGetValue(headerName, out var values))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate = "ApiKey";
            await context.Response.WriteAsJsonAsync(new
            {
                error = ApiErrors.ApiKey.MissingCode,
                message = $"{ApiErrors.ApiKey.MissingMessage} '{headerName}'."
            });
            return;
        }

        var providedKey = values.ToString();

        if (string.IsNullOrWhiteSpace(providedKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate = "ApiKey";
            await context.Response.WriteAsJsonAsync(new
            {
                error = ApiErrors.ApiKey.MissingCode,
                message = $"{ApiErrors.ApiKey.MissingMessage} '{headerName}'."
            });
            return;
        }

        var isValid = _options.Keys.Any(k => string.Equals(k, providedKey, StringComparison.Ordinal));

        if (!isValid)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                error = ApiErrors.ApiKey.InvalidCode,
                message = ApiErrors.ApiKey.InvalidMessage
            });
            return;
        }

        await next(context);
    }
}
