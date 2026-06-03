using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using MediatR;

namespace AppointmentSaaS.Application.Features.Appointments.Commands;

public record CompleteAppointmentCommand(Guid AppointmentId) : IRequest;

public class CompleteAppointmentCommandHandler(
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteAppointmentCommand>
{
    public async Task Handle(CompleteAppointmentCommand request, CancellationToken ct)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId, ct)
            ?? throw new NotFoundException(nameof(Appointment), request.AppointmentId);

        appointment.Complete();
        await appointmentRepository.UpdateAsync(appointment, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
