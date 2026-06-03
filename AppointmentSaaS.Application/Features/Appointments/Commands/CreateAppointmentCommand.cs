using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Appointments;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Exceptions;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Appointments.Commands;

public record CreateAppointmentCommand(Guid ServiceId, Guid StaffId, DateTime StartTime, string? Notes) : IRequest<AppointmentDto>;

public class CreateAppointmentCommandHandler(
    IAppointmentRepository appointmentRepository,
    IRepository<Service> serviceRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateAppointmentCommand, AppointmentDto>
{
    public async Task<AppointmentDto> Handle(CreateAppointmentCommand request, CancellationToken ct)
    {
        var service = await serviceRepository.GetByIdAsync(request.ServiceId, ct)
            ?? throw new NotFoundException(nameof(Service), request.ServiceId);

        var tenantId = currentUserService.TenantId
            ?? throw new ForbiddenException("Tenant context is required.");

        var clientId = Guid.Parse(currentUserService.UserId
            ?? throw new ForbiddenException("User context is required."));

        var endTime = request.StartTime.AddMinutes(service.DurationMinutes);

        var hasConflict = await appointmentRepository.HasConflictAsync(request.StaffId, request.StartTime, endTime, null, ct);
        if (hasConflict) throw new AppointmentConflictException(request.StartTime, endTime);

        var appointment = Appointment.Create(tenantId, request.ServiceId, request.StaffId, clientId, request.StartTime, endTime, request.Notes);
        await appointmentRepository.AddAsync(appointment, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<AppointmentDto>(appointment);
    }
}
