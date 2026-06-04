using AppointmentSaaS.Domain.Common;
using AppointmentSaaS.Domain.Enums;

namespace AppointmentSaaS.Domain.Entities;

/// <summary>
/// Represents a physical or logical business location that belongs to a <see cref="Tenant"/>.
/// <para>
/// A single Tenant may operate multiple Business locations (e.g., a salon chain with several
/// branches). Each Business acts as an organisational scope for <see cref="Staff"/>,
/// <see cref="Service"/>s, and <see cref="Appointment"/>s within that Tenant.
/// </para>
/// </summary>
public class Business : SoftDeleteEntity
{
    /// <summary>Foreign key to the owning <see cref="Tenant"/>.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Human-readable name for this location (e.g., "Downtown Branch").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>The type of business (e.g., Doctor, Salon, Consultant).</summary>
    public BusinessType Type { get; private set; } = BusinessType.Generic;

    /// <summary>Optional street address.</summary>
    public string? Address { get; private set; }

    /// <summary>City or locality where this business is located.</summary>
    public string? City { get; private set; }

    /// <summary>Contact phone number for this business location.</summary>
    public string? Phone { get; private set; }

    /// <summary>Contact email address for this business location.</summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Whether this business location is currently accepting bookings.
    /// Inactive locations are hidden from the booking UI.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>The owning tenant.</summary>
    public Tenant? Tenant { get; private set; }

    /// <summary>Staff members assigned to this location.</summary>
    public ICollection<Staff> Staff { get; private set; } = [];

    /// <summary>Services offered at this location.</summary>
    public ICollection<Service> Services { get; private set; } = [];

    /// <summary>Appointments booked at this location.</summary>
    public ICollection<Appointment> Appointments { get; private set; } = [];

    private Business() { }

    /// <summary>
    /// Creates a new <see cref="Business"/> location for the specified tenant.
    /// </summary>
    public static Business Create(
        Guid tenantId,
        string name,
        BusinessType type = BusinessType.Generic,
        string? address = null,
        string? city = null,
        string? phone = null,
        string? email = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Business
        {
            TenantId = tenantId,
            Name = name,
            Type = type,
            Address = address,
            City = city,
            Phone = phone,
            Email = email
        };
    }

    /// <summary>Updates mutable properties.</summary>
    public void Update(string name, BusinessType type, string? address, string? city, string? phone, string? email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Type = type;
        Address = address;
        City = city;
        Phone = phone;
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Prevents new bookings from being placed at this location.</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Allows bookings to be placed at this location again.</summary>
    public void Activate() => IsActive = true;
}
