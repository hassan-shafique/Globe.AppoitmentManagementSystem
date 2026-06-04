using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Businesses.Commands;

public record DeactivateBusinessCommand(Guid Id) : IRequest;

public class DeactivateBusinessCommandHandler(
    IBusinessRepository businessRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateBusinessCommand>
{
    public async Task Handle(DeactivateBusinessCommand request, CancellationToken ct)
    {
        var business = await businessRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Business), request.Id);

        business.Deactivate();
        await businessRepository.UpdateAsync(business, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
