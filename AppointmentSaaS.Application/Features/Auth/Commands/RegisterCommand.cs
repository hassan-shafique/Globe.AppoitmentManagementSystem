using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Auth;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Enums;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Auth.Commands;

public record RegisterCommand(string FirstName, string LastName, string Email, string Password, string TenantSlug) : IRequest<LoginResponse>;

public class RegisterCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    ITenantRepository tenantRepository,
    IRepository<AppUser> appUserRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        var tenant = await tenantRepository.GetBySlugAsync(request.TenantSlug, ct)
            ?? throw new NotFoundException(nameof(Tenant), request.TenantSlug);

        var (success, identityUserId, error) = await identityService.CreateUserAsync(request.Email, request.Password);

        if (!success || identityUserId is null)
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("Email", error ?? "Registration failed.")]);

        await identityService.AddToRoleAsync(identityUserId, UserRole.Client.ToString());

        var appUser = AppUser.Create(identityUserId, tenant.Id, request.FirstName, request.LastName, request.Email, UserRole.Client);
        await appUserRepository.AddAsync(appUser, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var roles = await identityService.GetRolesAsync(identityUserId);
        var accessToken = tokenService.GenerateAccessToken(appUser, request.Email, roles);
        var refreshToken = tokenService.GenerateRefreshToken(appUser.Id);

        return new LoginResponse(accessToken, refreshToken.Token, DateTime.UtcNow.AddMinutes(60),
            appUser.Id.ToString(), appUser.Email, appUser.FullName, UserRole.Client.ToString(), tenant.Id);
    }
}
