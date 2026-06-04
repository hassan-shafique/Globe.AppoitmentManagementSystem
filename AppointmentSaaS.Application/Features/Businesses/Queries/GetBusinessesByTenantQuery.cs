using AppointmentSaaS.Application.DTOs.Businesses;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Businesses.Queries;

public record GetBusinessesByTenantQuery(Guid TenantId) : IRequest<IReadOnlyList<BusinessDto>>;

public class GetBusinessesByTenantQueryHandler(
    IBusinessRepository businessRepository,
    IMapper mapper)
    : IRequestHandler<GetBusinessesByTenantQuery, IReadOnlyList<BusinessDto>>
{
    public async Task<IReadOnlyList<BusinessDto>> Handle(GetBusinessesByTenantQuery request, CancellationToken ct)
    {
        var businesses = await businessRepository.GetByTenantAsync(request.TenantId, ct);
        return mapper.Map<IReadOnlyList<BusinessDto>>(businesses);
    }
}
