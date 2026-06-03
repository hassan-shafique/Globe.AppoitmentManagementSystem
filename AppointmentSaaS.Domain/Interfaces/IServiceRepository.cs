using AppointmentSaaS.Domain.Entities;

namespace AppointmentSaaS.Domain.Interfaces;

public interface IServiceRepository : IRepository<Service>
{
    Task<IReadOnlyList<Service>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Service>> GetByBusinessAsync(Guid businessId, CancellationToken ct = default);
    Task<IReadOnlyList<Service>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default);
}
