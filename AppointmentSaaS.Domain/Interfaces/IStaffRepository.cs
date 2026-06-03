using AppointmentSaaS.Domain.Entities;

namespace AppointmentSaaS.Domain.Interfaces;

public interface IStaffRepository : IRepository<Staff>
{
    Task<IReadOnlyList<Staff>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Staff>> GetByBusinessAsync(Guid businessId, CancellationToken ct = default);
    Task<Staff?> GetByIdentityUserAsync(string identityUserId, CancellationToken ct = default);
}
