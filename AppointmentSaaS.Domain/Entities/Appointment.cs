using AppointmentSaaS.Domain.Common;
using AppointmentSaaS.Domain.Enums;
using AppointmentSaaS.Domain.Events;
using AppointmentSaaS.Domain.Exceptions;

namespace AppointmentSaaS.Domain.Entities;

/// <summary>
/// Aggregate root representing a single appointment booking.
/// <para>
/// An appointment ties a <see cref="Customer"/> (or a registered <see cref="AppUser"/> client)
/// to a <see cref="Staff"/> member for a specific <see cref="Service"/> within a defined
/// time window. The aggregate enforces its own lifecycle state machine and raises domain
/// events for side-effect handling (notifications, analytics, etc.).
/// </para>
/// <para>
/// State machine:
/// <code>
/// PENDING → Confirm() → CONFIRMED → Complete() → COMPLETED
///                     ↘ Cancel()  → CANCELLED
///                     ↘ MarkNoShow() → NO_SHOW
/// </code>
/// </para>
/// <para>
/// Either <see cref="CustomerId"/> (for a <see cref="Customer"/> record) or
/// <see cref="ClientId"/> (for a registered <see cref="AppUser"/>) must be supplied;
/// the factory method enforces this invariant.
/// </para>
/// </summary>
public class Appointment : AuditableEntity
{
    /// <summary>Foreign key to the <see cref="Tenant"/> that owns this appointment.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Foreign key to the <see cref="Service"/> being delivered.</summary>
    public Guid ServiceId { get; private set; }

    /// <summary>Foreign key to the <see cref="Staff"/> member delivering the service.</summary>
    public Guid StaffId { get; private set; }

    /// <summary>
    /// Foreign key to the <see cref="AppUser"/> (registered client).
    /// Mutually exclusive with <see cref="CustomerId"/> — one must be set.
    /// </summary>
    public Guid? ClientId { get; private set; }

    /// <summary>
    /// Foreign key to the <see cref="Customer"/> record (registered or walk-in).
    /// Mutually exclusive with <see cref="ClientId"/> — one must be set.
    /// </summary>
    public Guid? CustomerId { get; private set; }

    /// <summary>
    /// Optional foreign key to the <see cref="Business"/> location where the appointment
    /// will take place. <c>null</c> when the tenant has only one location.
    /// </summary>
    public Guid? BusinessId { get; private set; }

    /// <summary>UTC start time of the appointment.</summary>
    public DateTime StartTime { get; private set; }

    /// <summary>UTC end time of the appointment. Always &gt; <see cref="StartTime"/>.</summary>
    public DateTime EndTime { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public AppointmentStatus Status { get; private set; } = AppointmentStatus.Pending;

    /// <summary>Optional notes supplied by the client at booking time.</summary>
    public string? Notes { get; private set; }

    /// <summary>Reason provided when the appointment is cancelled. Set by <see cref="Cancel"/>.</summary>
    public string? CancellationReason { get; private set; }

    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>The owning tenant.</summary>
    public Tenant? Tenant { get; private set; }

    /// <summary>The service being delivered.</summary>
    public Service? Service { get; private set; }

    /// <summary>The staff member assigned to this appointment.</summary>
    public Staff? Staff { get; private set; }

    /// <summary>The registered user client, if booked via a user account.</summary>
    public AppUser? Client { get; private set; }

    /// <summary>The customer record, if booked as a walk-in or via the customer entity.</summary>
    public Customer? Customer { get; private set; }

    /// <summary>The business location where this appointment takes place, if applicable.</summary>
    public Business? Business { get; private set; }

    private Appointment() { }

    /// <summary>
    /// Creates a new <see cref="Appointment"/> for a registered <see cref="AppUser"/> client.
    /// Raises <see cref="AppointmentCreatedEvent"/>.
    /// </summary>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="serviceId">Service to be delivered.</param>
    /// <param name="staffId">Assigned staff member.</param>
    /// <param name="clientId">Registered client user ID.</param>
    /// <param name="startTime">UTC start time.</param>
    /// <param name="endTime">UTC end time. Must be after <paramref name="startTime"/>.</param>
    /// <param name="notes">Optional booking notes.</param>
    /// <param name="businessId">Optional business location.</param>
    public static Appointment Create(
        Guid tenantId,
        Guid serviceId,
        Guid staffId,
        Guid clientId,
        DateTime startTime,
        DateTime endTime,
        string? notes = null,
        Guid? businessId = null)
    {
        if (endTime <= startTime) throw new DomainException("End time must be after start time.");
        var appointment = new Appointment
        {
            TenantId = tenantId,
            ServiceId = serviceId,
            StaffId = staffId,
            ClientId = clientId,
            StartTime = startTime,
            EndTime = endTime,
            Notes = notes,
            BusinessId = businessId
        };
        appointment.AddDomainEvent(new AppointmentCreatedEvent(appointment.Id, tenantId, clientId));
        return appointment;
    }

    /// <summary>
    /// Creates a new <see cref="Appointment"/> for a <see cref="Customer"/> record
    /// (walk-in or non-portal booking). Raises <see cref="AppointmentCreatedEvent"/>.
    /// </summary>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="serviceId">Service to be delivered.</param>
    /// <param name="staffId">Assigned staff member.</param>
    /// <param name="customerId">Customer record ID.</param>
    /// <param name="startTime">UTC start time.</param>
    /// <param name="endTime">UTC end time. Must be after <paramref name="startTime"/>.</param>
    /// <param name="notes">Optional booking notes.</param>
    /// <param name="businessId">Optional business location.</param>
    public static Appointment CreateForCustomer(
        Guid tenantId,
        Guid serviceId,
        Guid staffId,
        Guid customerId,
        DateTime startTime,
        DateTime endTime,
        string? notes = null,
        Guid? businessId = null)
    {
        if (endTime <= startTime) throw new DomainException("End time must be after start time.");
        var appointment = new Appointment
        {
            TenantId = tenantId,
            ServiceId = serviceId,
            StaffId = staffId,
            CustomerId = customerId,
            StartTime = startTime,
            EndTime = endTime,
            Notes = notes,
            BusinessId = businessId
        };
        appointment.AddDomainEvent(new AppointmentCreatedEvent(appointment.Id, tenantId, customerId));
        return appointment;
    }

    /// <summary>
    /// Confirms a pending appointment.
    /// </summary>
    /// <exception cref="DomainException">Thrown if the appointment is not in <see cref="AppointmentStatus.Pending"/> state.</exception>
    public void Confirm()
    {
        if (Status != AppointmentStatus.Pending)
            throw new DomainException("Only pending appointments can be confirmed.");
        Status = AppointmentStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the appointment with a mandatory reason.
    /// Raises <see cref="AppointmentCancelledEvent"/>.
    /// </summary>
    /// <param name="reason">Human-readable cancellation reason.</param>
    /// <exception cref="DomainException">Thrown if the appointment is already completed or cancelled.</exception>
    public void Cancel(string reason)
    {
        if (Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            throw new DomainException("Cannot cancel a completed or already cancelled appointment.");
        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new AppointmentCancelledEvent(Id, TenantId, reason));
    }

    /// <summary>
    /// Marks the appointment as completed.
    /// </summary>
    /// <exception cref="DomainException">Thrown if the appointment is not confirmed or in progress.</exception>
    public void Complete()
    {
        if (Status != AppointmentStatus.InProgress && Status != AppointmentStatus.Confirmed)
            throw new DomainException("Only confirmed or in-progress appointments can be completed.");
        Status = AppointmentStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a no-show for a confirmed appointment where the client did not attend.
    /// </summary>
    /// <exception cref="DomainException">Thrown if the appointment is not confirmed.</exception>
    public void MarkNoShow()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new DomainException("Only confirmed appointments can be marked as no-show.");
        Status = AppointmentStatus.NoShow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns <c>true</c> if this appointment's time window overlaps with the given range.
    /// Used by conflict-detection logic before creating a new appointment for the same staff member.
    /// </summary>
    /// <param name="start">Proposed start time.</param>
    /// <param name="end">Proposed end time.</param>
    public bool OverlapsWith(DateTime start, DateTime end) =>
        StartTime < end && EndTime > start;
}
