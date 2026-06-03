using AppointmentSaaS.Application.Common.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Auth.Commands;

public record ForgotPasswordCommand(string Email) : IRequest<Unit>;

public class ForgotPasswordCommandHandler(
    IIdentityService identityService,
    IEmailService emailService)
    : IRequestHandler<ForgotPasswordCommand, Unit>
{
    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var (_, token, _) = await identityService.GeneratePasswordResetTokenAsync(request.Email);

        // Only send email when a token was generated (user exists); avoids user enumeration
        if (token is not null)
            await emailService.SendPasswordResetAsync(request.Email, request.Email, token, ct);

        return Unit.Value;
    }
}
