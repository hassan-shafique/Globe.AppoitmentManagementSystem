using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.DTOs.Services;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Services.Queries;

public record GetServiceByIdQuery(Guid Id) : IRequest<ServiceDto>;

public class GetServiceByIdQueryHandler(
    IServiceRepository serviceRepository,
    IMapper mapper)
    : IRequestHandler<GetServiceByIdQuery, ServiceDto>
{
    public async Task<ServiceDto> Handle(GetServiceByIdQuery request, CancellationToken ct)
    {
        var service = await serviceRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Service), request.Id);

        return mapper.Map<ServiceDto>(service);
    }
}
