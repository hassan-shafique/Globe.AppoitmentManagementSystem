namespace AppointmentSaaS.Domain.Events;

public record AppointmentCancelledEvent(Guid AppointmentId, Guid TenantId, string Reason) : DomainEvent;
