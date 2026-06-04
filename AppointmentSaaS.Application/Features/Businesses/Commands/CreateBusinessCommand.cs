using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Businesses;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Enums;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Businesses.Commands;

public record CreateBusinessCommand(
    string Name,
    BusinessType Type,
    string? Address,
    string? City,
    string? Phone,
    string? Email) : IRequest<BusinessDto>;

public class CreateBusinessCommandHandler(
    IBusinessRepository businessRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateBusinessCommand, BusinessDto>
{
    public async Task<BusinessDto> Handle(CreateBusinessCommand request, CancellationToken ct)
    {
        var tenantId = currentUserService.TenantId
            ?? throw new ForbiddenException("Tenant context is required.");

        var business = Business.Create(tenantId, request.Name, request.Type, request.Address, request.City, request.Phone, request.Email);
        await businessRepository.AddAsync(business, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<BusinessDto>(business);
    }
}
