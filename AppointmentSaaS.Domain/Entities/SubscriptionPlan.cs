using AppointmentSaaS.Domain.Common;
using AppointmentSaaS.Domain.Exceptions;

namespace AppointmentSaaS.Domain.Entities;

/// <summary>
/// Defines a subscription tier offered to <see cref="Tenant"/>s (e.g., Free, Pro, Enterprise).
/// <para>
/// Plans enforce usage limits such as maximum staff members, appointments per month, and
/// business locations. A <c>null</c> limit means the feature is unlimited on that tier.
/// Tenants reference the active plan via <see cref="Tenant.SubscriptionPlanId"/>.
/// </para>
/// </summary>
public class SubscriptionPlan : AuditableEntity
{
    /// <summary>Display name for this plan tier (e.g., "Professional").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Marketing description shown to prospective subscribers.</summary>
    public string? Description { get; private set; }

    /// <summary>Recurring monthly price in the platform's base currency.</summary>
    public decimal MonthlyPrice { get; private set; }

    /// <summary>
    /// Discounted annual price (billed once per year).
    /// Typically lower than <c>MonthlyPrice * 12</c>.
    /// </summary>
    public decimal AnnualPrice { get; private set; }

    /// <summary>
    /// Maximum number of active staff members permitted under this plan.
    /// <c>null</c> means unlimited.
    /// </summary>
    public int? MaxStaffMembers { get; private set; }

    /// <summary>
    /// Maximum appointments that may be created per calendar month.
    /// <c>null</c> means unlimited.
    /// </summary>
    public int? MaxAppointmentsPerMonth { get; private set; }

    /// <summary>
    /// Maximum number of <see cref="Business"/> locations a tenant may register.
    /// <c>null</c> means unlimited.
    /// </summary>
    public int? MaxBusinessLocations { get; private set; }

    /// <summary>
    /// Whether this plan is currently visible and available for new subscriptions.
    /// Inactive plans retain existing subscribers but accept no new sign-ups.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>All tenants currently subscribed to this plan.</summary>
    public ICollection<Tenant> Tenants { get; private set; } = [];

    private SubscriptionPlan() { }

    /// <summary>
    /// Creates a new <see cref="SubscriptionPlan"/>.
    /// </summary>
    /// <param name="name">Plan tier name. Must not be empty.</param>
    /// <param name="monthlyPrice">Monthly recurring price. Must be ≥ 0.</param>
    /// <param name="annualPrice">Annual (once-yearly) price. Must be ≥ 0.</param>
    /// <param name="description">Optional marketing description.</param>
    /// <param name="maxStaff">Max active staff members; <c>null</c> for unlimited.</param>
    /// <param name="maxAppointmentsPerMonth">Max appointments/month; <c>null</c> for unlimited.</param>
    /// <param name="maxLocations">Max business locations; <c>null</c> for unlimited.</param>
    public static SubscriptionPlan Create(
        string name,
        decimal monthlyPrice,
        decimal annualPrice,
        string? description = null,
        int? maxStaff = null,
        int? maxAppointmentsPerMonth = null,
        int? maxLocations = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (monthlyPrice < 0) throw new DomainException("Monthly price cannot be negative.");
        if (annualPrice < 0) throw new DomainException("Annual price cannot be negative.");
        return new SubscriptionPlan
        {
            Name = name,
            Description = description,
            MonthlyPrice = monthlyPrice,
            AnnualPrice = annualPrice,
            MaxStaffMembers = maxStaff,
            MaxAppointmentsPerMonth = maxAppointmentsPerMonth,
            MaxBusinessLocations = maxLocations
        };
    }

    /// <summary>Updates plan details and limits. Sets <see cref="AuditableEntity.UpdatedAt"/>.</summary>
    public void Update(
        string name,
        string? description,
        decimal monthlyPrice,
        decimal annualPrice,
        int? maxStaff,
        int? maxAppointmentsPerMonth,
        int? maxLocations)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (monthlyPrice < 0) throw new DomainException("Monthly price cannot be negative.");
        if (annualPrice < 0) throw new DomainException("Annual price cannot be negative.");
        Name = name;
        Description = description;
        MonthlyPrice = monthlyPrice;
        AnnualPrice = annualPrice;
        MaxStaffMembers = maxStaff;
        MaxAppointmentsPerMonth = maxAppointmentsPerMonth;
        MaxBusinessLocations = maxLocations;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Hides this plan from new subscribers while retaining existing ones.</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Makes this plan available for new subscriptions again.</summary>
    public void Activate() => IsActive = true;
}
