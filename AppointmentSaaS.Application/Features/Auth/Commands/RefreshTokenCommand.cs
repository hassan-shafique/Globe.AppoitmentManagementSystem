using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Auth;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<LoginResponse>;

public class RefreshTokenCommandHandler(
    ITokenService tokenService,
    IRepository<AppUser> appUserRepository,
    IRepository<RefreshToken> refreshTokenRepository,
    IIdentityService identityService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var userId = tokenService.GetUserIdFromExpiredToken(request.AccessToken)
            ?? throw new ValidationException([new FluentValidation.Results.ValidationFailure("Token", "Invalid access token.")]);

        var appUser = await appUserRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException(nameof(AppUser), userId);

        var tokens = await refreshTokenRepository.FindAsync(
            t => t.AppUserId == userId && t.Token == request.RefreshToken, ct);
        var storedToken = tokens.FirstOrDefault();

        if (storedToken == null || !storedToken.IsActive)
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("RefreshToken", "Invalid or expired refresh token.")]);

        var newRefreshToken = tokenService.GenerateRefreshToken(appUser.Id);
        storedToken.Revoke("Replaced", newRefreshToken.Token);
        await refreshTokenRepository.AddAsync(newRefreshToken, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var roles = await identityService.GetRolesAsync(appUser.IdentityUserId);
        var accessToken = tokenService.GenerateAccessToken(appUser, appUser.Email, roles);

        return new LoginResponse(accessToken, newRefreshToken.Token, DateTime.UtcNow.AddMinutes(60),
            appUser.Id.ToString(), appUser.Email, appUser.FullName,
            roles.FirstOrDefault() ?? "Client", appUser.TenantId);
    }
}
