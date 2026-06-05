namespace AppointmentSaaS.Application.DTOs.Staff;

public record CreateStaffRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone = null,
    string? Bio = null,
    string? Role = null,
    string? Skills = null,
    Guid? BusinessId = null);
