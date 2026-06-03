using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using AppointmentSaaS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSaaS.Infrastructure.Repositories;

public class CustomerRepository(AppDbContext context)
    : Repository<Customer>(context), ICustomerRepository
{
    public async Task<IReadOnlyList<Customer>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default) =>
        await DbSet
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .ToListAsync(ct);

    public async Task<Customer?> GetByEmailAsync(Guid tenantId, string email, CancellationToken ct = default) =>
        await DbSet
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Email == email, ct);

    public async Task<Customer?> GetByAppUserAsync(Guid appUserId, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(c => c.AppUserId == appUserId, ct);
}
