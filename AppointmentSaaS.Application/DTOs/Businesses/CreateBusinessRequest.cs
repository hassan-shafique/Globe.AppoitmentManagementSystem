using AppointmentSaaS.Domain.Enums;

namespace AppointmentSaaS.Application.DTOs.Businesses;

public record CreateBusinessRequest(
    string Name,
    BusinessType Type,
    string? Address,
    string? City,
    string? Phone,
    string? Email);
