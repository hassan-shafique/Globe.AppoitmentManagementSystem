using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Staff.Commands;

public record DeleteStaffCommand(Guid Id) : IRequest;

public class DeleteStaffCommandHandler(
    IStaffRepository staffRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteStaffCommand>
{
    public async Task Handle(DeleteStaffCommand request, CancellationToken ct)
    {
        var staff = await staffRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Staff), request.Id);

        staff.SoftDelete();
        await staffRepository.UpdateAsync(staff, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
