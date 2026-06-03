using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace AppointmentSaaS.Infrastructure.Services;

public class IdentityService(
    UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<(bool Success, string? UserId, IList<string> Roles, string? Error)> ValidateCredentialsAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return (false, null, [], "Invalid credentials.");

        if (!await userManager.CheckPasswordAsync(user, password))
            return (false, null, [], "Invalid credentials.");

        if (!user.EmailConfirmed)
            return (false, null, [], "Email address has not been verified.");

        var roles = await userManager.GetRolesAsync(user);
        return (true, user.Id, roles, null);
    }

    public async Task<(bool Success, string? UserId, string? Error)> CreateUserAsync(string email, string password, string? firstName = null, string? lastName = null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = false
        };
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var error = string.Join("; ", result.Errors.Select(e => e.Description));
            return (false, null, error);
        }

        return (true, user.Id, null);
    }

    public async Task AddToRoleAsync(string identityUserId, string role)
    {
        var user = await userManager.FindByIdAsync(identityUserId);
        if (user is not null) await userManager.AddToRoleAsync(user, role);
    }

    public async Task<IList<string>> GetRolesAsync(string identityUserId)
    {
        var user = await userManager.FindByIdAsync(identityUserId);
        return user is null ? [] : await userManager.GetRolesAsync(user);
    }

    public async Task<(bool Success, string? Token, string? Error)> GenerateEmailVerificationTokenAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return (false, null, "User not found.");

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        return (true, token, null);
    }

    public async Task<(bool Success, string? Error)> ConfirmEmailAsync(string email, string token)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return (false, "User not found.");

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            var error = string.Join("; ", result.Errors.Select(e => e.Description));
            return (false, error);
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Token, string? Error)> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        // Return success even if user not found to avoid user enumeration
        if (user is null) return (true, null, null);

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return (true, token, null);
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return (false, "Invalid password reset request.");

        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            var error = string.Join("; ", result.Errors.Select(e => e.Description));
            return (false, error);
        }

        return (true, null);
    }
}
