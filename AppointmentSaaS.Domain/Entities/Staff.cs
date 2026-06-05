using AppointmentSaaS.Domain.Common;

namespace AppointmentSaaS.Domain.Entities;

/// <summary>
/// Represents a staff member who delivers <see cref="Service"/>s within a <see cref="Tenant"/>.
/// <para>
/// Staff members have an optional <see cref="IdentityUserId"/> link that grants them
/// portal access. The <see cref="Appointment"/> aggregate references staff via
/// <see cref="Appointment.StaffId"/> and conflict detection is performed per staff member.
/// </para>
/// <para>
/// A staff member may optionally be assigned to a specific <see cref="Business"/> location.
/// Omitting <see cref="BusinessId"/> indicates tenant-wide availability.
/// </para>
/// </summary>
public class Staff : SoftDeleteEntity
{
    /// <summary>Foreign key to the owning <see cref="Tenant"/>.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Optional link to the ASP.NET Identity user account for portal access.
    /// Staff without an identity account can still be assigned appointments by an admin.
    /// </summary>
    public string IdentityUserId { get; private set; } = string.Empty;

    /// <summary>
    /// Optional foreign key to the <see cref="Business"/> location this staff member
    /// is primarily assigned to. <c>null</c> means tenant-wide availability.
    /// </summary>
    public Guid? BusinessId { get; private set; }

    /// <summary>Given name.</summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>Family name.</summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>Contact email shown on the staff profile.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Optional contact phone number.</summary>
    public string? Phone { get; private set; }

    /// <summary>Optional bio or short description displayed in the booking UI.</summary>
    public string? Bio { get; private set; }

    /// <summary>Job role or title (e.g. "Doctor", "Receptionist").</summary>
    public string? Role { get; private set; }

    /// <summary>Comma-separated list of skills or specialisations (e.g. "Botox,Fillers").</summary>
    public string? Skills { get; private set; }

    /// <summary>Whether this staff member is currently available for booking.</summary>
    public bool IsActive { get; private set; } = true;

    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>The owning tenant.</summary>
    public Tenant? Tenant { get; private set; }

    /// <summary>The business location this staff member is primarily assigned to, if any.</summary>
    public Business? Business { get; private set; }

    /// <summary>All appointments assigned to this staff member.</summary>
    public ICollection<Appointment> Appointments { get; private set; } = [];

    private Staff() { }

    /// <summary>
    /// Creates a new <see cref="Staff"/> member.
    /// </summary>
    public static Staff Create(
        Guid tenantId,
        string identityUserId,
        string firstName,
        string lastName,
        string email,
        string? phone = null,
        string? role = null,
        string? skills = null,
        Guid? businessId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return new Staff
        {
            TenantId = tenantId,
            IdentityUserId = identityUserId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Role = role,
            Skills = skills,
            BusinessId = businessId
        };
    }

    /// <summary>Full name composed of <see cref="FirstName"/> and <see cref="LastName"/>.</summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>Updates profile fields. Sets <see cref="AuditableEntity.UpdatedAt"/>.</summary>
    public void Update(string firstName, string lastName, string email, string? phone, string? bio, string? role, string? skills)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Bio = bio;
        Role = role;
        Skills = skills;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Reassigns this staff member to a different (or no) business location.</summary>
    public void AssignToBusiness(Guid? businessId)
    {
        BusinessId = businessId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Marks the staff member as unavailable for new bookings.</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Marks the staff member as available for bookings.</summary>
    public void Activate() => IsActive = true;
}
