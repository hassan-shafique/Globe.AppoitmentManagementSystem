using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Appointments.Commands;

public record CancelAppointmentCommand(Guid AppointmentId, string Reason) : IRequest;

public class CancelAppointmentCommandHandler(
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CancelAppointmentCommand>
{
    public async Task Handle(CancelAppointmentCommand request, CancellationToken ct)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId, ct)
            ?? throw new NotFoundException(nameof(Appointment), request.AppointmentId);

        appointment.Cancel(request.Reason);
        await appointmentRepository.UpdateAsync(appointment, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
