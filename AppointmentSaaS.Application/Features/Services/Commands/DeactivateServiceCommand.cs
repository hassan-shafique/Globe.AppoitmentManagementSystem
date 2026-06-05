using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Services.Commands;

public record DeactivateServiceCommand(Guid Id) : IRequest;

public class DeactivateServiceCommandHandler(
    IServiceRepository serviceRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateServiceCommand>
{
    public async Task Handle(DeactivateServiceCommand request, CancellationToken ct)
    {
        var service = await serviceRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Service), request.Id);

        service.Deactivate();
        await serviceRepository.UpdateAsync(service, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
