using AppointmentSaaS.Application.DTOs.Services;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Services.Queries;

public record GetServicesByBusinessQuery(Guid BusinessId) : IRequest<IReadOnlyList<ServiceDto>>;

public class GetServicesByBusinessQueryHandler(
    IServiceRepository serviceRepository,
    IMapper mapper)
    : IRequestHandler<GetServicesByBusinessQuery, IReadOnlyList<ServiceDto>>
{
    public async Task<IReadOnlyList<ServiceDto>> Handle(GetServicesByBusinessQuery request, CancellationToken ct)
    {
        var services = await serviceRepository.GetByBusinessAsync(request.BusinessId, ct);
        return mapper.Map<IReadOnlyList<ServiceDto>>(services);
    }
}
