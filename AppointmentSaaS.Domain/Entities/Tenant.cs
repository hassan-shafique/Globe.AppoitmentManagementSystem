using AppointmentSaaS.Domain.Common;

namespace AppointmentSaaS.Domain.Entities;

/// <summary>
/// Aggregate root representing an organisation (tenant) that subscribes to the platform.
/// <para>
/// All data-scoped entities (users, staff, services, appointments, customers) are isolated
/// by <see cref="Id"/>. The <see cref="Slug"/> provides a URL-friendly, globally unique
/// identifier for tenant routing (e.g., <c>/acme-salon/book</c>).
/// </para>
/// <para>
/// Tenants can optionally be placed on a <see cref="SubscriptionPlan"/> that enforces
/// usage limits. Tenants may also register multiple <see cref="Business"/> locations.
/// </para>
/// </summary>
public class Tenant : SoftDeleteEntity
{
    /// <summary>Display name of the organisation.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// URL-safe, lowercase, globally unique identifier (e.g., <c>acme-salon</c>).
    /// Used in subdomain routing and public booking URLs.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Optional URL of the tenant's logo image.</summary>
    public string? LogoUrl { get; private set; }

    /// <summary>Primary contact email for the organisation.</summary>
    public string? ContactEmail { get; private set; }

    /// <summary>Primary contact phone number.</summary>
    public string? ContactPhone { get; private set; }

    /// <summary>
    /// Optional link to the <see cref="SubscriptionPlan"/> this tenant is subscribed to.
    /// <c>null</c> indicates no active plan (e.g., during trial or free tier).
    /// </summary>
    public Guid? SubscriptionPlanId { get; private set; }

    /// <summary>
    /// Whether this tenant is currently active. Inactive tenants cannot create new
    /// appointments or log in, but their data is preserved.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>The subscription plan this tenant is on, if any.</summary>
    public SubscriptionPlan? SubscriptionPlan { get; private set; }

    /// <summary>Portal and staff user accounts belonging to this tenant.</summary>
    public ICollection<AppUser> Users { get; private set; } = [];

    /// <summary>Services offered by this tenant.</summary>
    public ICollection<Service> Services { get; private set; } = [];

    /// <summary>Staff members employed by this tenant.</summary>
    public ICollection<Staff> Staff { get; private set; } = [];

    /// <summary>Business/location entities under this tenant.</summary>
    public ICollection<Business> Businesses { get; private set; } = [];

    /// <summary>Customer records owned by this tenant.</summary>
    public ICollection<Customer> Customers { get; private set; } = [];

    /// <summary>All appointments across this tenant's businesses.</summary>
    public ICollection<Appointment> Appointments { get; private set; } = [];

    private Tenant() { }

    /// <summary>
    /// Creates a new <see cref="Tenant"/>.
    /// </summary>
    /// <param name="name">Organisation name. Must not be empty.</param>
    /// <param name="slug">URL slug. Must not be empty; automatically lower-cased.</param>
    /// <param name="contactEmail">Primary contact email.</param>
    public static Tenant Create(string name, string slug, string contactEmail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        return new Tenant { Name = name, Slug = slug.ToLowerInvariant(), ContactEmail = contactEmail };
    }

    /// <summary>Updates mutable display and contact fields. Sets <see cref="AuditableEntity.UpdatedAt"/>.</summary>
    public void Update(string name, string? logoUrl, string? contactEmail, string? contactPhone)
    {
        Name = name;
        LogoUrl = logoUrl;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Assigns or changes the subscription plan.
    /// Pass <c>null</c> to remove the current plan (e.g., on cancellation).
    /// </summary>
    public void SetSubscriptionPlan(Guid? planId)
    {
        SubscriptionPlanId = planId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Suspends this tenant — users cannot log in and bookings are blocked.</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Reinstates a previously suspended tenant.</summary>
    public void Activate() => IsActive = true;
}
