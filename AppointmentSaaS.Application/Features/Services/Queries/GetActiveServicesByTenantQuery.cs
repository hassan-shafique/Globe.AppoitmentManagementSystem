using AppointmentSaaS.Application.DTOs.Services;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Services.Queries;

public record GetActiveServicesByTenantQuery(Guid TenantId) : IRequest<IReadOnlyList<ServiceDto>>;

public class GetActiveServicesByTenantQueryHandler(
    IServiceRepository serviceRepository,
    IMapper mapper)
    : IRequestHandler<GetActiveServicesByTenantQuery, IReadOnlyList<ServiceDto>>
{
    public async Task<IReadOnlyList<ServiceDto>> Handle(GetActiveServicesByTenantQuery request, CancellationToken ct)
    {
        var services = await serviceRepository.GetActiveByTenantAsync(request.TenantId, ct);
        return mapper.Map<IReadOnlyList<ServiceDto>>(services);
    }
}
