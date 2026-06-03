namespace AppointmentSaaS.Application.DTOs.Services;

public record CreateServiceRequest(string Name, string? Description, int DurationMinutes, decimal Price);
