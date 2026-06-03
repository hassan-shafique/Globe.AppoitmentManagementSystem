using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.DTOs.Tenants;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Tenants.Commands;

public record CreateTenantCommand(string Name, string Slug, string ContactEmail) : IRequest<TenantDto>;

public class CreateTenantCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateTenantCommand, TenantDto>
{
    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken ct)
    {
        var slugExists = await tenantRepository.SlugExistsAsync(request.Slug, ct);
        if (slugExists)
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("Slug", $"Slug '{request.Slug}' is already taken.")]);

        var tenant = Tenant.Create(request.Name, request.Slug, request.ContactEmail);
        await tenantRepository.AddAsync(tenant, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<TenantDto>(tenant);
    }
}
