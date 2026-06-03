using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Auth.Commands;

public record RevokeTokenCommand(string Token) : IRequest;

public class RevokeTokenCommandHandler(
    IRepository<RefreshToken> refreshTokenRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RevokeTokenCommand>
{
    public async Task Handle(RevokeTokenCommand request, CancellationToken ct)
    {
        var tokens = await refreshTokenRepository.FindAsync(t => t.Token == request.Token, ct);
        var token = tokens.FirstOrDefault()
            ?? throw new NotFoundException(nameof(RefreshToken), request.Token);

        if (!token.IsActive)
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("Token", "Token is already revoked.")]);

        token.Revoke("Manually revoked");
        await refreshTokenRepository.UpdateAsync(token, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
