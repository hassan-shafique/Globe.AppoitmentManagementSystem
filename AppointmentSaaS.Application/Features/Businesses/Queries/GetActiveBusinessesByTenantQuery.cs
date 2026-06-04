using AppointmentSaaS.Application.DTOs.Businesses;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Businesses.Queries;

public record GetActiveBusinessesByTenantQuery(Guid TenantId) : IRequest<IReadOnlyList<BusinessDto>>;

public class GetActiveBusinessesByTenantQueryHandler(
    IBusinessRepository businessRepository,
    IMapper mapper)
    : IRequestHandler<GetActiveBusinessesByTenantQuery, IReadOnlyList<BusinessDto>>
{
    public async Task<IReadOnlyList<BusinessDto>> Handle(GetActiveBusinessesByTenantQuery request, CancellationToken ct)
    {
        var businesses = await businessRepository.GetActiveByTenantAsync(request.TenantId, ct);
        return mapper.Map<IReadOnlyList<BusinessDto>>(businesses);
    }
}
