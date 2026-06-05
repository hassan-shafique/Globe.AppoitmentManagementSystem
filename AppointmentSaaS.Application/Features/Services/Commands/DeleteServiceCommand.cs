using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Services.Commands;

public record DeleteServiceCommand(Guid Id) : IRequest;

public class DeleteServiceCommandHandler(
    IServiceRepository serviceRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteServiceCommand>
{
    public async Task Handle(DeleteServiceCommand request, CancellationToken ct)
    {
        var service = await serviceRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Service), request.Id);

        service.SoftDelete();
        await serviceRepository.UpdateAsync(service, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
