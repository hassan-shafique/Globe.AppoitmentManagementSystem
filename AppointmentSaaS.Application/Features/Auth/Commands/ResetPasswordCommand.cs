using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using FluentValidation.Results;
using MediatR;

namespace AppointmentSaaS.Application.Features.Auth.Commands;

public record ResetPasswordCommand(string Email, string Token, string NewPassword, string ConfirmPassword) : IRequest<Unit>;

public class ResetPasswordCommandHandler(IIdentityService identityService)
    : IRequestHandler<ResetPasswordCommand, Unit>
{
    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        if (request.NewPassword != request.ConfirmPassword)
            throw new ValidationException([new ValidationFailure("ConfirmPassword", "Passwords do not match.")]);

        var (success, error) = await identityService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);

        if (!success)
            throw new ValidationException([new ValidationFailure("Token", error ?? "Password reset failed.")]);

        return Unit.Value;
    }
}
