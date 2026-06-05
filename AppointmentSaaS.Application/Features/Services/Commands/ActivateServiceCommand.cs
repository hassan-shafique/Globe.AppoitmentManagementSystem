using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Services.Commands;

public record ActivateServiceCommand(Guid Id) : IRequest;

public class ActivateServiceCommandHandler(
    IServiceRepository serviceRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ActivateServiceCommand>
{
    public async Task Handle(ActivateServiceCommand request, CancellationToken ct)
    {
        var service = await serviceRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Service), request.Id);

        service.Activate();
        await serviceRepository.UpdateAsync(service, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
