using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Auth;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Enums;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Auth.Commands;

public record RegisterCommand(string FirstName, string LastName, string Email, string Password, string TenantSlug) : IRequest<RegisterResponse>;

public class RegisterCommandHandler(
    IIdentityService identityService,
    IEmailService emailService,
    ITenantRepository tenantRepository,
    IRepository<AppUser> appUserRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        var tenant = await tenantRepository.GetBySlugAsync(request.TenantSlug, ct)
            ?? throw new NotFoundException(nameof(Tenant), request.TenantSlug);

        var (success, identityUserId, error) = await identityService.CreateUserAsync(
            request.Email, request.Password, request.FirstName, request.LastName);

        if (!success || identityUserId is null)
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("Email", error ?? "Registration failed.")]);

        await identityService.AddToRoleAsync(identityUserId, UserRole.Client.ToString());

        var appUser = AppUser.Create(identityUserId, tenant.Id, request.FirstName, request.LastName, request.Email, UserRole.Client);
        await appUserRepository.AddAsync(appUser, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var (_, verificationToken, _) = await identityService.GenerateEmailVerificationTokenAsync(request.Email);
        if (verificationToken is not null)
            await emailService.SendEmailVerificationAsync(request.Email, appUser.FullName, verificationToken, ct);

        return new RegisterResponse(appUser.Id.ToString(), appUser.Email, appUser.FullName,
            "Registration successful. Please check your email to verify your account.");
    }
}
