namespace AppointmentSaaS.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendAppointmentConfirmationAsync(string toEmail, string clientName, DateTime appointmentTime, string serviceName, CancellationToken ct = default);
    Task SendAppointmentCancellationAsync(string toEmail, string clientName, DateTime appointmentTime, string reason, CancellationToken ct = default);
    Task SendWelcomeEmailAsync(string toEmail, string name, CancellationToken ct = default);
}
