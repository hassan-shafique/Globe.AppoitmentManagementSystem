using AppointmentSaaS.Domain.Common;

namespace AppointmentSaaS.Domain.Entities;

/// <summary>
/// Represents a customer (client) who books <see cref="Appointment"/>s.
/// <para>
/// A <see cref="Customer"/> record may exist without a corresponding identity account,
/// which enables walk-in or admin-created bookings. When a customer self-registers,
/// <see cref="AppUserId"/> links to the matching <see cref="AppUser"/>, providing a
/// single source of truth for their booking history across both authenticated and
/// unauthenticated flows.
/// </para>
/// </summary>
public class Customer : SoftDeleteEntity
{
    /// <summary>Foreign key to the <see cref="Tenant"/> that owns this customer record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Optional link to a registered <see cref="AppUser"/>. <c>null</c> for walk-in or
    /// admin-created customers who do not have a portal account.
    /// </summary>
    public Guid? AppUserId { get; private set; }

    /// <summary>Customer's given name.</summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>Customer's family name.</summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>Primary contact email address.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Optional contact phone number.</summary>
    public string? Phone { get; private set; }

    /// <summary>
    /// Internal notes visible only to staff (e.g., allergies, preferences).
    /// Never exposed in customer-facing views.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Whether this customer record is active. Deactivated customers cannot book new
    /// appointments but their history is preserved.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>The tenant that owns this customer record.</summary>
    public Tenant? Tenant { get; private set; }

    /// <summary>The identity-linked user account, if any.</summary>
    public AppUser? AppUser { get; private set; }

    /// <summary>All appointments booked by this customer.</summary>
    public ICollection<Appointment> Appointments { get; private set; } = [];

    private Customer() { }

    /// <summary>
    /// Creates a new <see cref="Customer"/> record scoped to a tenant.
    /// </summary>
    /// <param name="tenantId">Id of the tenant who owns this record.</param>
    /// <param name="firstName">Given name. Must not be empty.</param>
    /// <param name="lastName">Family name. Must not be empty.</param>
    /// <param name="email">Primary email address. Must not be empty.</param>
    /// <param name="phone">Optional phone number.</param>
    /// <param name="notes">Optional internal notes.</param>
    /// <param name="appUserId">Optional link to a registered <see cref="AppUser"/>.</param>
    public static Customer Create(
        Guid tenantId,
        string firstName,
        string lastName,
        string email,
        string? phone = null,
        string? notes = null,
        Guid? appUserId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return new Customer
        {
            TenantId = tenantId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Notes = notes,
            AppUserId = appUserId
        };
    }

    /// <summary>Full name composed of <see cref="FirstName"/> and <see cref="LastName"/>.</summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>Updates contact details. Sets <see cref="AuditableEntity.UpdatedAt"/>.</summary>
    public void Update(string firstName, string lastName, string email, string? phone, string? notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Prevents this customer from booking new appointments.</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Allows this customer to book appointments again.</summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// Links this customer to a registered <see cref="AppUser"/> account.
    /// Can only be set once — linking an already-linked customer is a no-op.
    /// </summary>
    public void LinkToAppUser(Guid appUserId)
    {
        if (AppUserId.HasValue) return;
        AppUserId = appUserId;
        UpdatedAt = DateTime.UtcNow;
    }
}
