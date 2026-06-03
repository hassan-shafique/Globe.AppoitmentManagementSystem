namespace AppointmentSaaS.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool Success, string? UserId, IList<string> Roles, string? Error)> ValidateCredentialsAsync(string email, string password);
    Task<(bool Success, string? UserId, string? Error)> CreateUserAsync(string email, string password, string? firstName = null, string? lastName = null);
    Task AddToRoleAsync(string identityUserId, string role);
    Task<IList<string>> GetRolesAsync(string identityUserId);

    // Email verification
    Task<(bool Success, string? Token, string? Error)> GenerateEmailVerificationTokenAsync(string email);
    Task<(bool Success, string? Error)> ConfirmEmailAsync(string email, string token);

    // Password reset
    Task<(bool Success, string? Token, string? Error)> GeneratePasswordResetTokenAsync(string email);
    Task<(bool Success, string? Error)> ResetPasswordAsync(string email, string token, string newPassword);
}
