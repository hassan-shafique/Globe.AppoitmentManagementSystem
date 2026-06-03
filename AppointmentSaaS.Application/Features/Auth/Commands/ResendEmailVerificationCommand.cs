using AppointmentSaaS.Application.Common.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Auth.Commands;

public record ResendEmailVerificationCommand(string Email) : IRequest<Unit>;

public class ResendEmailVerificationCommandHandler(
    IIdentityService identityService,
    IEmailService emailService)
    : IRequestHandler<ResendEmailVerificationCommand, Unit>
{
    public async Task<Unit> Handle(ResendEmailVerificationCommand request, CancellationToken ct)
    {
        var (_, token, _) = await identityService.GenerateEmailVerificationTokenAsync(request.Email);

        if (token is not null)
            await emailService.SendEmailVerificationAsync(request.Email, request.Email, token, ct);

        return Unit.Value;
    }
}
