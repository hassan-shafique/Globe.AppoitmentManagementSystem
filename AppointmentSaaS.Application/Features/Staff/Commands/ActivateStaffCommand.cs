using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Staff.Commands;

public record ActivateStaffCommand(Guid Id) : IRequest;

public class ActivateStaffCommandHandler(
    IStaffRepository staffRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ActivateStaffCommand>
{
    public async Task Handle(ActivateStaffCommand request, CancellationToken ct)
    {
        var staff = await staffRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Staff), request.Id);

        staff.Activate();
        await staffRepository.UpdateAsync(staff, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
