using AppointmentSaaS.Domain.Common;
using AppointmentSaaS.Domain.Enums;

namespace AppointmentSaaS.Domain.Entities;

/// <summary>
/// Represents an application user scoped to a specific <see cref="Tenant"/>.
/// <para>
/// Every user in the system has a corresponding ASP.NET Core Identity account
/// (<see cref="IdentityUserId"/>). This entity layers on top of Identity to carry
/// multi-tenant context, role information, and contact details. Staff and admin users
/// link here; customers may additionally have a <see cref="Customer"/> record.
/// </para>
/// </summary>
public class AppUser : SoftDeleteEntity
{
    /// <summary>
    /// The ASP.NET Core Identity user ID (string-typed GUID from <c>IdentityUser.Id</c>).
    /// Used to correlate JWT claims with domain-level permissions.
    /// </summary>
    public string IdentityUserId { get; private set; } = string.Empty;

    /// <summary>Foreign key to the <see cref="Tenant"/> this user belongs to.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Given name.</summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>Family name.</summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>Email address (mirrors the Identity account email).</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Optional contact phone number.</summary>
    public string? Phone { get; private set; }

    /// <summary>
    /// Role controlling what the user may do within the tenant.
    /// <see cref="UserRole.SuperAdmin"/> is platform-wide; other roles are tenant-scoped.
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>Whether this user account is currently active and permitted to sign in.</summary>
    public bool IsActive { get; private set; } = true;

    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>The tenant this user belongs to.</summary>
    public Tenant? Tenant { get; private set; }

    /// <summary>JWT refresh tokens associated with this user's sessions.</summary>
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

    private AppUser() { }

    /// <summary>
    /// Creates a new <see cref="AppUser"/> linked to an existing Identity account.
    /// </summary>
    /// <param name="identityUserId">The <c>IdentityUser.Id</c> string.</param>
    /// <param name="tenantId">Id of the tenant this user belongs to.</param>
    /// <param name="firstName">Given name.</param>
    /// <param name="lastName">Family name.</param>
    /// <param name="email">Email address.</param>
    /// <param name="role">Assigned role within the tenant.</param>
    public static AppUser Create(
        string identityUserId,
        Guid tenantId,
        string firstName,
        string lastName,
        string email,
        UserRole role)
    {
        return new AppUser
        {
            IdentityUserId = identityUserId,
            TenantId = tenantId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Role = role
        };
    }

    /// <summary>Full name composed of <see cref="FirstName"/> and <see cref="LastName"/>.</summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>Prevents this user from signing in.</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Allows this user to sign in again.</summary>
    public void Activate() => IsActive = true;
}
