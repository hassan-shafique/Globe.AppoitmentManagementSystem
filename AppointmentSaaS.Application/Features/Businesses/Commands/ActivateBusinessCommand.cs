using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Businesses.Commands;

public record ActivateBusinessCommand(Guid Id) : IRequest;

public class ActivateBusinessCommandHandler(
    IBusinessRepository businessRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ActivateBusinessCommand>
{
    public async Task Handle(ActivateBusinessCommand request, CancellationToken ct)
    {
        var business = await businessRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Business), request.Id);

        business.Activate();
        await businessRepository.UpdateAsync(business, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
