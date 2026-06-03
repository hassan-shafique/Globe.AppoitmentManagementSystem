using AppointmentSaaS.Application.DTOs.Appointments;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Appointments.Queries;

public record GetAppointmentsByTenantQuery(Guid TenantId) : IRequest<IReadOnlyList<AppointmentDto>>;

public class GetAppointmentsByTenantQueryHandler(
    IAppointmentRepository appointmentRepository,
    IMapper mapper)
    : IRequestHandler<GetAppointmentsByTenantQuery, IReadOnlyList<AppointmentDto>>
{
    public async Task<IReadOnlyList<AppointmentDto>> Handle(GetAppointmentsByTenantQuery request, CancellationToken ct)
    {
        var appointments = await appointmentRepository.GetByTenantAsync(request.TenantId, ct);
        return mapper.Map<IReadOnlyList<AppointmentDto>>(appointments);
    }
}
