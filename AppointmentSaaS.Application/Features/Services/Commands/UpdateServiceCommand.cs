using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.DTOs.Services;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Services.Commands;

public record UpdateServiceCommand(
    Guid Id,
    string Name,
    string? Description,
    int DurationMinutes,
    decimal Price,
    int BufferTimeMinutes) : IRequest<ServiceDto>;

public class UpdateServiceCommandHandler(
    IServiceRepository serviceRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdateServiceCommand, ServiceDto>
{
    public async Task<ServiceDto> Handle(UpdateServiceCommand request, CancellationToken ct)
    {
        var service = await serviceRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Service), request.Id);

        service.Update(request.Name, request.Description, request.DurationMinutes, request.Price, request.BufferTimeMinutes);
        await serviceRepository.UpdateAsync(service, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<ServiceDto>(service);
    }
}
