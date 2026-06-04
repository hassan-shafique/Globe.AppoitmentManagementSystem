namespace AppointmentSaaS.Application.Common.Interfaces;

/// <summary>
/// Provides the current tenant context for query filtering and data isolation.
/// Returns null when there is no authenticated tenant (e.g., background services or super-admin).
/// </summary>
public interface ITenantProvider
{
    Guid? TenantId { get; }
}
