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
    ITokenService tokenService,
    IRepository<AppUser> appUserRepository,
    ITenantRepository tenantRepository)
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

        var accessToken = tokenService.GenerateAccessToken(appUser, request.Email, roles);
        var refreshToken = tokenService.GenerateRefreshToken(appUser.Id);

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
