namespace AppointmentSaaS.Application.DTOs.Services;

public record ServiceDto(
    Guid Id,
    Guid TenantId,
    Guid? BusinessId,
    string Name,
    string? Description,
    int DurationMinutes,
    decimal Price,
    int BufferTimeMinutes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
