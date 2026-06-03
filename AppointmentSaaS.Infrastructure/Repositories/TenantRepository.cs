using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using AppointmentSaaS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSaaS.Infrastructure.Repositories;

public class TenantRepository(AppDbContext context)
    : Repository<Tenant>(context), ITenantRepository
{
    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(t => t.Slug == slug.ToLowerInvariant(), ct);

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default) =>
        await DbSet.AnyAsync(t => t.Slug == slug.ToLowerInvariant(), ct);
}
