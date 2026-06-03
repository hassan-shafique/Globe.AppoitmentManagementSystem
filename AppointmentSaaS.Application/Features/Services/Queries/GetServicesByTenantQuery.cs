using AppointmentSaaS.Application.DTOs.Services;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Services.Queries;

public record GetServicesByTenantQuery(Guid TenantId) : IRequest<IReadOnlyList<ServiceDto>>;

public class GetServicesByTenantQueryHandler(
    IRepository<Service> serviceRepository,
    IMapper mapper)
    : IRequestHandler<GetServicesByTenantQuery, IReadOnlyList<ServiceDto>>
{
    public async Task<IReadOnlyList<ServiceDto>> Handle(GetServicesByTenantQuery request, CancellationToken ct)
    {
        var services = await serviceRepository.FindAsync(s => s.TenantId == request.TenantId && s.IsActive, ct);
        return mapper.Map<IReadOnlyList<ServiceDto>>(services);
    }
}
