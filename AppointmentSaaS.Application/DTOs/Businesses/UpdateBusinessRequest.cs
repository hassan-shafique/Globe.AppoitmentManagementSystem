using AppointmentSaaS.Domain.Enums;

namespace AppointmentSaaS.Application.DTOs.Businesses;

public record UpdateBusinessRequest(
    string Name,
    BusinessType Type,
    string? Address,
    string? City,
    string? Phone,
    string? Email);
