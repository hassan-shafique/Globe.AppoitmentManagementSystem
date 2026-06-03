namespace AppointmentSaaS.Application.DTOs.Services;

public record ServiceDto(Guid Id, Guid TenantId, string Name, string? Description, int DurationMinutes, decimal Price, bool IsActive);
