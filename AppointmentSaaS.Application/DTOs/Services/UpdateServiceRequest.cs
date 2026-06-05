namespace AppointmentSaaS.Application.DTOs.Services;

public record UpdateServiceRequest(
    string Name,
    string? Description,
    int DurationMinutes,
    decimal Price,
    int BufferTimeMinutes = 0);
