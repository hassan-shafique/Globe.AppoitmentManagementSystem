namespace AppointmentSaaS.Application.DTOs.Tenants;

public record CreateTenantRequest(string Name, string Slug, string ContactEmail);
