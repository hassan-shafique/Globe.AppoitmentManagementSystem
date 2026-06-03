namespace AppointmentSaaS.Application.DTOs.Auth;

public record VerifyEmailRequest(string Email, string Token);
