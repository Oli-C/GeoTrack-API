namespace GeoTrack.API.Common;

public sealed class TenantContext
{
    public Guid TenantId { get; private set; }
    public bool HasTenant => TenantId != Guid.Empty;

    public void SetTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId must not be empty.", nameof(tenantId));

        TenantId = tenantId;
    }
}
