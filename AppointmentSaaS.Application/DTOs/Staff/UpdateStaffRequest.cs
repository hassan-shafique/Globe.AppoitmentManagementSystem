namespace AppointmentSaaS.Application.DTOs.Staff;

public record UpdateStaffRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone = null,
    string? Bio = null,
    string? Role = null,
    string? Skills = null);
