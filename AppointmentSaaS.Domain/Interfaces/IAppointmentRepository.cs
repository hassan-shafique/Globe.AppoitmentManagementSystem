using AppointmentSaaS.Domain.Entities;

namespace AppointmentSaaS.Domain.Interfaces;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IReadOnlyList<Appointment>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Appointment>> GetByStaffAsync(Guid staffId, DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<Appointment>> GetByClientAsync(Guid clientId, CancellationToken ct = default);
    Task<bool> HasConflictAsync(Guid staffId, DateTime startTime, DateTime endTime, Guid? excludeAppointmentId = null, CancellationToken ct = default);
}
