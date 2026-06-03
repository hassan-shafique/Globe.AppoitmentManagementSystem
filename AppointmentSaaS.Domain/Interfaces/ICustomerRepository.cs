using AppointmentSaaS.Domain.Entities;

namespace AppointmentSaaS.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<IReadOnlyList<Customer>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<Customer?> GetByEmailAsync(Guid tenantId, string email, CancellationToken ct = default);
    Task<Customer?> GetByAppUserAsync(Guid appUserId, CancellationToken ct = default);
}
