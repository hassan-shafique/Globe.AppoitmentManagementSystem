using AppointmentSaaS.Domain.Entities;

namespace AppointmentSaaS.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(AppUser user, string email, IList<string> roles);
    RefreshToken GenerateRefreshToken(Guid appUserId);
    Guid? GetUserIdFromExpiredToken(string token);
}
