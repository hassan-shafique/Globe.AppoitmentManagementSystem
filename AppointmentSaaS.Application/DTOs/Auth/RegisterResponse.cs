namespace AppointmentSaaS.Application.DTOs.Auth;

public record RegisterResponse(string UserId, string Email, string FullName, string Message);
