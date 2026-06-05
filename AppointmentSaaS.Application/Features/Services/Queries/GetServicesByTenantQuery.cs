using AppointmentSaaS.Application.DTOs.Services;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Services.Queries;

public record GetServicesByTenantQuery(Guid TenantId) : IRequest<IReadOnlyList<ServiceDto>>;

public class GetServicesByTenantQueryHandler(
    IServiceRepository serviceRepository,
    IMapper mapper)
    : IRequestHandler<GetServicesByTenantQuery, IReadOnlyList<ServiceDto>>
{
    public async Task<IReadOnlyList<ServiceDto>> Handle(GetServicesByTenantQuery request, CancellationToken ct)
    {
        var services = await serviceRepository.GetByTenantAsync(request.TenantId, ct);
        return mapper.Map<IReadOnlyList<ServiceDto>>(services);
    }
}
