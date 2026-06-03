using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Services;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Services.Commands;

public record CreateServiceCommand(string Name, string? Description, int DurationMinutes, decimal Price) : IRequest<ServiceDto>;

public class CreateServiceCommandHandler(
    IRepository<Service> serviceRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateServiceCommand, ServiceDto>
{
    public async Task<ServiceDto> Handle(CreateServiceCommand request, CancellationToken ct)
    {
        var tenantId = currentUserService.TenantId
            ?? throw new Common.Exceptions.ForbiddenException("Tenant context required.");

        var service = Service.Create(tenantId, request.Name, request.Description, request.DurationMinutes, request.Price);
        await serviceRepository.AddAsync(service, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<ServiceDto>(service);
    }
}
