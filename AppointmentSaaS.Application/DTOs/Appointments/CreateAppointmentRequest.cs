namespace AppointmentSaaS.Application.DTOs.Appointments;

public record CreateAppointmentRequest(
    Guid ServiceId,
    Guid StaffId,
    DateTime StartTime,
    string? Notes);
