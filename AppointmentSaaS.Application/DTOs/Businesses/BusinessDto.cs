using AppointmentSaaS.Domain.Enums;

namespace AppointmentSaaS.Application.DTOs.Businesses;

public record BusinessDto(
    Guid Id,
    Guid TenantId,
    string Name,
    BusinessType Type,
    string? Address,
    string? City,
    string? Phone,
    string? Email,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
