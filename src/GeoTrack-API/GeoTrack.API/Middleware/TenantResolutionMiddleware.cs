using GeoTrack.API.Common;
using GeoTrack.API.Data;
using Microsoft.EntityFrameworkCore;

namespace GeoTrack.API.Middleware;

public sealed class TenantResolutionMiddleware : IMiddleware
{
    private readonly TrackingDbContext _db;

    public TenantResolutionMiddleware(TrackingDbContext db)
    {
        _db = db;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Allow endpoints to omit tenant header (e.g. health/docs). Tenant-required endpoints
        // should check TenantContext.HasTenant.
        if (!context.Request.Headers.TryGetValue(ApiHeaders.TenantId, out var values))
        {
            await next(context);
            return;
        }

        var raw = values.ToString();

        if (!Guid.TryParse(raw, out var tenantId) || tenantId == Guid.Empty)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = ApiErrors.Tenant.InvalidCode,
                message = $"{ApiErrors.Tenant.InvalidMessage} '{ApiHeaders.TenantId}'."
            });
            return;
        }

        // Strong validation: tenant must exist.
        var exists = await _db.Tenants.AsNoTracking().AnyAsync(t => t.Id == tenantId);
        if (!exists)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = ApiErrors.Tenant.InvalidCode,
                message = $"{ApiErrors.Tenant.InvalidMessage} '{ApiHeaders.TenantId}'."
            });
            return;
        }

        var tenantContext = context.RequestServices.GetRequiredService<TenantContext>();
        tenantContext.SetTenant(tenantId);

        await next(context);
    }
}
