namespace AppointmentSaaS.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool Success, string? UserId, IList<string> Roles, string? Error)> ValidateCredentialsAsync(string email, string password);
    Task<(bool Success, string? UserId, string? Error)> CreateUserAsync(string email, string password);
    Task AddToRoleAsync(string identityUserId, string role);
    Task<IList<string>> GetRolesAsync(string identityUserId);
}
