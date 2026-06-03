namespace AppointmentSaaS.Application.DTOs.Tenants;

public record TenantDto(Guid Id, string Name, string Slug, string? LogoUrl, string? ContactEmail, string? ContactPhone, bool IsActive, DateTime CreatedAt);
