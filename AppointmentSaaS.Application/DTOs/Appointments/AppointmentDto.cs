using AppointmentSaaS.Domain.Enums;

namespace AppointmentSaaS.Application.DTOs.Appointments;

public record AppointmentDto(
    Guid Id,
    Guid TenantId,
    Guid ServiceId,
    string ServiceName,
    Guid StaffId,
    string StaffName,
    Guid ClientId,
    string ClientName,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status,
    string? Notes,
    string? CancellationReason,
    DateTime CreatedAt);
