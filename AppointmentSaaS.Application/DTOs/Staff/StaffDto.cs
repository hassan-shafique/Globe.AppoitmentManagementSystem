namespace AppointmentSaaS.Application.DTOs.Staff;

public record StaffDto(Guid Id, Guid TenantId, string FirstName, string LastName, string Email, string? Phone, string? Bio, bool IsActive);
