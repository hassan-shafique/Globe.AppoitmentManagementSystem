using AppointmentSaaS.Domain.Common;

namespace AppointmentSaaS.Domain.Entities;

/// <summary>
/// Represents a bookable service offering within a <see cref="Tenant"/>.
/// <para>
/// Services define what can be scheduled: the name, how long the appointment takes
/// (<see cref="DurationMinutes"/>), and the price charged. They may optionally be scoped
/// to a specific <see cref="Business"/> location. Inactive services are hidden from the
/// booking UI but retain historical appointment references.
/// </para>
/// </summary>
public class Service : SoftDeleteEntity
{
    /// <summary>Foreign key to the owning <see cref="Tenant"/>.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Optional foreign key to the <see cref="Business"/> location where this service is offered.
    /// <c>null</c> means available at all locations within the tenant.
    /// </summary>
    public Guid? BusinessId { get; private set; }

    /// <summary>Display name of the service (e.g., "Haircut &amp; Blow Dry").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Optional description shown in the booking UI.</summary>
    public string? Description { get; private set; }

    /// <summary>
    /// How long the service takes, in minutes. Determines the appointment end time.
    /// Must be a positive integer.
    /// </summary>
    public int DurationMinutes { get; private set; }

    /// <summary>
    /// Price of the service in the platform's base currency.
    /// Zero indicates a free/complimentary service.
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Time blocked after the appointment ends before the next booking can start, in minutes.
    /// Zero means no buffer.
    /// </summary>
    public int BufferTimeMinutes { get; private set; }

    /// <summary>Whether this service is currently available for booking.</summary>
    public bool IsActive { get; private set; } = true;

    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>The tenant that owns this service.</summary>
    public Tenant? Tenant { get; private set; }

    /// <summary>The business location this service is offered at, if restricted to one.</summary>
    public Business? Business { get; private set; }

    /// <summary>All appointments that have been booked for this service.</summary>
    public ICollection<Appointment> Appointments { get; private set; } = [];

    private Service() { }

    /// <summary>
    /// Creates a new <see cref="Service"/>.
    /// </summary>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="name">Service name. Must not be empty.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="durationMinutes">Duration in minutes. Must be &gt; 0.</param>
    /// <param name="price">Price. Must be ≥ 0.</param>
    /// <param name="bufferTimeMinutes">Minutes to block after appointment ends. Defaults to 0.</param>
    /// <param name="businessId">Optional business location restriction.</param>
    public static Service Create(
        Guid tenantId,
        string name,
        string? description,
        int durationMinutes,
        decimal price,
        int bufferTimeMinutes = 0,
        Guid? businessId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (durationMinutes <= 0) throw new ArgumentException("Duration must be positive.", nameof(durationMinutes));
        if (price < 0) throw new ArgumentException("Price cannot be negative.", nameof(price));
        if (bufferTimeMinutes < 0) throw new ArgumentException("Buffer time cannot be negative.", nameof(bufferTimeMinutes));
        return new Service
        {
            TenantId = tenantId,
            Name = name,
            Description = description,
            DurationMinutes = durationMinutes,
            Price = price,
            BufferTimeMinutes = bufferTimeMinutes,
            BusinessId = businessId
        };
    }

    /// <summary>Updates mutable fields. Sets <see cref="AuditableEntity.UpdatedAt"/>.</summary>
    public void Update(string name, string? description, int durationMinutes, decimal price, int bufferTimeMinutes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (durationMinutes <= 0) throw new ArgumentException("Duration must be positive.", nameof(durationMinutes));
        if (price < 0) throw new ArgumentException("Price cannot be negative.", nameof(price));
        if (bufferTimeMinutes < 0) throw new ArgumentException("Buffer time cannot be negative.", nameof(bufferTimeMinutes));
        Name = name;
        Description = description;
        DurationMinutes = durationMinutes;
        Price = price;
        BufferTimeMinutes = bufferTimeMinutes;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Hides this service from the booking UI without removing appointment history.</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Makes this service available for booking again.</summary>
    public void Activate() => IsActive = true;
}
