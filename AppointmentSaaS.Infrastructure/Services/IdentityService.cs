using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace AppointmentSaaS.Infrastructure.Services;

public class IdentityService(
    UserManager<ApplicationIdentityUser> userManager) : IIdentityService
{
    public async Task<(bool Success, string? UserId, IList<string> Roles, string? Error)> ValidateCredentialsAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return (false, null, [], "Invalid credentials.");

        if (!await userManager.CheckPasswordAsync(user, password))
            return (false, null, [], "Invalid credentials.");

        var roles = await userManager.GetRolesAsync(user);
        return (true, user.Id, roles, null);
    }

    public async Task<(bool Success, string? UserId, string? Error)> CreateUserAsync(string email, string password)
    {
        var user = new ApplicationIdentityUser { UserName = email, Email = email, EmailConfirmed = true };
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
}
