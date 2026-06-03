using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Auth;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password, string TenantSlug) : IRequest<LoginResponse>;

public class LoginCommandHandler(
    IIdentityService identityService,
    IJwtService jwtService,
    IRepository<AppUser> appUserRepository,
    IRepository<RefreshToken> refreshTokenRepository,
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var tenant = await tenantRepository.GetBySlugAsync(request.TenantSlug, ct)
            ?? throw new NotFoundException(nameof(Tenant), request.TenantSlug);

        var (success, identityUserId, roles, error) = await identityService.ValidateCredentialsAsync(request.Email, request.Password);

        if (!success || identityUserId is null)
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("Email", "Invalid credentials.")]);

        var appUsers = await appUserRepository.FindAsync(
            u => u.IdentityUserId == identityUserId && u.TenantId == tenant.Id, ct);
        var appUser = appUsers.FirstOrDefault()
            ?? throw new ForbiddenException("User does not belong to this tenant.");

        var accessToken = jwtService.GenerateAccessToken(appUser, request.Email, roles);
        var refreshToken = jwtService.GenerateRefreshToken(appUser.Id);

        await refreshTokenRepository.AddAsync(refreshToken, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new LoginResponse(
            accessToken,
            refreshToken.Token,
            DateTime.UtcNow.AddMinutes(60),
            appUser.Id.ToString(),
            appUser.Email,
            appUser.FullName,
            roles.FirstOrDefault() ?? "Client",
            tenant.Id);
    }
}
