using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using AppointmentSaaS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSaaS.Infrastructure.Repositories;

public class BusinessRepository(AppDbContext context)
    : Repository<Business>(context), IBusinessRepository
{
    public async Task<IReadOnlyList<Business>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default) =>
        await DbSet
            .Where(b => b.TenantId == tenantId)
            .OrderBy(b => b.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Business>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default) =>
        await DbSet
            .Where(b => b.TenantId == tenantId && b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync(ct);
}
