using AppointmentSaaS.Domain.Events;

namespace AppointmentSaaS.Domain.Common;

/// <summary>
/// Abstract root base for every domain entity in the system.
/// <para>
/// Provides a stable <see cref="Id"/> (auto-generated <see cref="Guid"/>) and an in-memory
/// collection of <see cref="DomainEvent"/> instances that are dispatched by the infrastructure
/// layer after <c>SaveChangesAsync</c> succeeds. Entities should not dispatch events themselves;
/// they only enqueue them.
/// </para>
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Gets the unique identifier for this entity. Assigned at construction time.</summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private readonly List<DomainEvent> _domainEvents = [];

    /// <summary>
    /// Read-only snapshot of domain events raised by this entity and not yet dispatched.
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Enqueues a domain event to be dispatched after the current unit of work commits.</summary>
    public void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Removes a previously enqueued domain event (e.g., on cancellation).</summary>
    public void RemoveDomainEvent(DomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    /// <summary>Clears all pending domain events. Called by the infrastructure dispatcher after publishing.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
