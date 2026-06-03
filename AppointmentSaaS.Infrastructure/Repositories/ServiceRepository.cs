using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using AppointmentSaaS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSaaS.Infrastructure.Repositories;

public class ServiceRepository(AppDbContext context)
    : Repository<Service>(context), IServiceRepository
{
    public async Task<IReadOnlyList<Service>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default) =>
        await DbSet
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Service>> GetByBusinessAsync(Guid businessId, CancellationToken ct = default) =>
        await DbSet
            .Where(s => s.BusinessId == businessId)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Service>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default) =>
        await DbSet
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
}
