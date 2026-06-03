namespace AppointmentSaaS.Application.DTOs.Appointments;

public record UpdateAppointmentRequest(Guid Id, string? Notes);
