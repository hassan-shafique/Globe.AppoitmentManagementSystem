using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.DTOs.Tenants;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Tenants.Queries;

public record GetTenantBySlugQuery(string Slug) : IRequest<TenantDto>;

public class GetTenantBySlugQueryHandler(
    ITenantRepository tenantRepository,
    IMapper mapper)
    : IRequestHandler<GetTenantBySlugQuery, TenantDto>
{
    public async Task<TenantDto> Handle(GetTenantBySlugQuery request, CancellationToken ct)
    {
        var tenant = await tenantRepository.GetBySlugAsync(request.Slug, ct)
            ?? throw new NotFoundException(nameof(Tenant), request.Slug);
        return mapper.Map<TenantDto>(tenant);
    }
}
