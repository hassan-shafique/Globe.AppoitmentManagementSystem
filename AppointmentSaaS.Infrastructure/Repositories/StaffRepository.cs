using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using AppointmentSaaS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSaaS.Infrastructure.Repositories;

public class StaffRepository(AppDbContext context)
    : Repository<Staff>(context), IStaffRepository
{
    public async Task<IReadOnlyList<Staff>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default) =>
        await DbSet
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.LastName).ThenBy(s => s.FirstName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Staff>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default) =>
        await DbSet
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .OrderBy(s => s.LastName).ThenBy(s => s.FirstName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Staff>> GetByBusinessAsync(Guid businessId, CancellationToken ct = default) =>
        await DbSet
            .Where(s => s.BusinessId == businessId)
            .OrderBy(s => s.LastName).ThenBy(s => s.FirstName)
            .ToListAsync(ct);

    public async Task<Staff?> GetByIdentityUserAsync(string identityUserId, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(s => s.IdentityUserId == identityUserId, ct);
}
