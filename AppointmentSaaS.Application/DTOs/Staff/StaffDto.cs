namespace AppointmentSaaS.Application.DTOs.Staff;

public record StaffDto(
    Guid Id,
    Guid TenantId,
    Guid? BusinessId,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? Phone,
    string? Bio,
    string? Role,
    string? Skills,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
