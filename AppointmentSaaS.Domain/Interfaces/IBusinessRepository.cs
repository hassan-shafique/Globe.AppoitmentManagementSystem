using AppointmentSaaS.Domain.Entities;

namespace AppointmentSaaS.Domain.Interfaces;

public interface IBusinessRepository : IRepository<Business>
{
    Task<IReadOnlyList<Business>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Business>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default);
}
