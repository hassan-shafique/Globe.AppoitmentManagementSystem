using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using AppointmentSaaS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSaaS.Infrastructure.Repositories;

public class AppointmentRepository(AppDbContext context)
    : Repository<Appointment>(context), IAppointmentRepository
{
    public async Task<IReadOnlyList<Appointment>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default) =>
        await DbSet
            .Include(a => a.Service)
            .Include(a => a.Staff)
            .Include(a => a.Client)
            .Where(a => a.TenantId == tenantId)
            .OrderByDescending(a => a.StartTime)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Appointment>> GetByStaffAsync(Guid staffId, DateTime from, DateTime to, CancellationToken ct = default) =>
        await DbSet
            .Where(a => a.StaffId == staffId && a.StartTime >= from && a.EndTime <= to)
            .OrderBy(a => a.StartTime)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Appointment>> GetByClientAsync(Guid clientId, CancellationToken ct = default) =>
        await DbSet
            .Include(a => a.Service)
            .Include(a => a.Staff)
            .Where(a => a.ClientId == clientId)
            .OrderByDescending(a => a.StartTime)
            .ToListAsync(ct);

    public async Task<bool> HasConflictAsync(Guid staffId, DateTime startTime, DateTime endTime, Guid? excludeAppointmentId = null, CancellationToken ct = default)
    {
        var query = DbSet.Where(a =>
            a.StaffId == staffId &&
            a.Status != Domain.Enums.AppointmentStatus.Cancelled &&
            a.Status != Domain.Enums.AppointmentStatus.NoShow &&
            a.StartTime < endTime &&
            a.EndTime > startTime);

        if (excludeAppointmentId.HasValue)
            query = query.Where(a => a.Id != excludeAppointmentId.Value);

        return await query.AnyAsync(ct);
    }
}
