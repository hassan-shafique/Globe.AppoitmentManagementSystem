using AppointmentSaaS.Domain.Common;

namespace AppointmentSaaS.Domain.Entities;

/// <summary>
/// A JWT refresh token tied to an <see cref="AppUser"/> session.
/// <para>
/// Refresh tokens are opaque, single-use strings stored in the database. On use, the
/// old token is revoked and a new one is issued (rotation). A token is considered active
/// only when it is neither revoked nor expired.
/// </para>
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>The opaque token string sent to the client. Treated as a secret — store hashed if possible.</summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>Foreign key to the owning <see cref="AppUser"/>.</summary>
    public Guid AppUserId { get; private set; }

    /// <summary>UTC expiry time. Tokens are rejected after this point even if not explicitly revoked.</summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>UTC creation time.</summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary><c>true</c> when the token has been explicitly revoked (used, logged out, or security reset).</summary>
    public bool IsRevoked { get; private set; }

    /// <summary>
    /// The token string that replaced this one during rotation, if applicable.
    /// Aids forensic analysis of token chains.
    /// </summary>
    public string? ReplacedByToken { get; private set; }

    /// <summary>Human-readable reason for revocation (e.g., "Used", "Logout", "SecurityReset").</summary>
    public string? RevokedReason { get; private set; }

    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>The user whose session this token represents.</summary>
    public AppUser? AppUser { get; private set; }

    /// <summary><c>true</c> when the token's expiry time has passed.</summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary><c>true</c> only when the token is both non-revoked and non-expired.</summary>
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    /// <summary>
    /// Creates a new <see cref="RefreshToken"/> for the specified user.
    /// </summary>
    /// <param name="appUserId">Owning user.</param>
    /// <param name="token">Opaque token string.</param>
    /// <param name="expiresAt">UTC expiry timestamp.</param>
    public static RefreshToken Create(Guid appUserId, string token, DateTime expiresAt)
    {
        return new RefreshToken { AppUserId = appUserId, Token = token, ExpiresAt = expiresAt };
    }

    /// <summary>
    /// Revokes this token, recording the reason and the replacement token (if rotating).
    /// </summary>
    /// <param name="reason">Reason for revocation.</param>
    /// <param name="replacedByToken">Token string that replaced this one, if applicable.</param>
    public void Revoke(string reason, string? replacedByToken = null)
    {
        IsRevoked = true;
        RevokedReason = reason;
        ReplacedByToken = replacedByToken;
    }
}
