using AppointmentSaaS.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AppointmentSaaS.Infrastructure.Services;

public class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public Task SendAppointmentConfirmationAsync(string toEmail, string clientName, DateTime appointmentTime, string serviceName, CancellationToken ct = default)
    {
        logger.LogInformation("Sending appointment confirmation to {Email} for {Service} at {Time}", toEmail, serviceName, appointmentTime);
        return Task.CompletedTask;
    }

    public Task SendAppointmentCancellationAsync(string toEmail, string clientName, DateTime appointmentTime, string reason, CancellationToken ct = default)
    {
        logger.LogInformation("Sending cancellation email to {Email}, reason: {Reason}", toEmail, reason);
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string toEmail, string name, CancellationToken ct = default)
    {
        logger.LogInformation("Sending welcome email to {Email}", toEmail);
        return Task.CompletedTask;
    }

    public Task SendEmailVerificationAsync(string toEmail, string name, string verificationToken, CancellationToken ct = default)
    {
        logger.LogInformation("Sending email verification to {Email} for {Name}", toEmail, name);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string toEmail, string name, string resetToken, CancellationToken ct = default)
    {
        logger.LogInformation("Sending password reset email to {Email}", toEmail);
        return Task.CompletedTask;
    }
}
