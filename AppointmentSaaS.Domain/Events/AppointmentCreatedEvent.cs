namespace AppointmentSaaS.Domain.Events;

public record AppointmentCreatedEvent(Guid AppointmentId, Guid TenantId, Guid ClientId) : DomainEvent;
