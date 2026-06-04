using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.DTOs.Businesses;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Businesses.Queries;

public record GetBusinessByIdQuery(Guid Id) : IRequest<BusinessDto>;

public class GetBusinessByIdQueryHandler(
    IBusinessRepository businessRepository,
    IMapper mapper)
    : IRequestHandler<GetBusinessByIdQuery, BusinessDto>
{
    public async Task<BusinessDto> Handle(GetBusinessByIdQuery request, CancellationToken ct)
    {
        var business = await businessRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Business), request.Id);

        return mapper.Map<BusinessDto>(business);
    }
}
