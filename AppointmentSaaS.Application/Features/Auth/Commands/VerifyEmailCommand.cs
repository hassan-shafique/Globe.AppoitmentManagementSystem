using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using FluentValidation.Results;
using MediatR;

namespace AppointmentSaaS.Application.Features.Auth.Commands;

public record VerifyEmailCommand(string Email, string Token) : IRequest<Unit>;

public class VerifyEmailCommandHandler(IIdentityService identityService)
    : IRequestHandler<VerifyEmailCommand, Unit>
{
    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken ct)
    {
        var (success, error) = await identityService.ConfirmEmailAsync(request.Email, request.Token);

        if (!success)
            throw new ValidationException([new ValidationFailure("Token", error ?? "Email verification failed.")]);

        return Unit.Value;
    }
}
