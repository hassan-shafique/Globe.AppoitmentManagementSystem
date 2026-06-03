using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Appointments.Commands;

public record ConfirmAppointmentCommand(Guid AppointmentId) : IRequest;

public class ConfirmAppointmentCommandHandler(
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ConfirmAppointmentCommand>
{
    public async Task Handle(ConfirmAppointmentCommand request, CancellationToken ct)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId, ct)
            ?? throw new NotFoundException(nameof(Appointment), request.AppointmentId);

        appointment.Confirm();
        await appointmentRepository.UpdateAsync(appointment, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
