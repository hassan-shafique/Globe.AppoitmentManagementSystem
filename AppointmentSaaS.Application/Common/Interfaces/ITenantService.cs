namespace AppointmentSaaS.Application.Common.Interfaces;

public interface ITenantService
{
    Guid? CurrentTenantId { get; }
    string? CurrentTenantSlug { get; }
    Task SetTenantAsync(string slug, CancellationToken ct = default);
}
