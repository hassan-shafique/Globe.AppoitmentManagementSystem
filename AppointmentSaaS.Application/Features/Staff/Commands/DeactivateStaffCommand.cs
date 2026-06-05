using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Staff.Commands;

public record DeactivateStaffCommand(Guid Id) : IRequest;

public class DeactivateStaffCommandHandler(
    IStaffRepository staffRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateStaffCommand>
{
    public async Task Handle(DeactivateStaffCommand request, CancellationToken ct)
    {
        var staff = await staffRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Staff), request.Id);

        staff.Deactivate();
        await staffRepository.UpdateAsync(staff, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
