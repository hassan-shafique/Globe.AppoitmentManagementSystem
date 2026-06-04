using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Businesses.Commands;

public record DeleteBusinessCommand(Guid Id) : IRequest;

public class DeleteBusinessCommandHandler(
    IBusinessRepository businessRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBusinessCommand>
{
    public async Task Handle(DeleteBusinessCommand request, CancellationToken ct)
    {
        var business = await businessRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Business), request.Id);

        business.SoftDelete();
        await businessRepository.UpdateAsync(business, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
