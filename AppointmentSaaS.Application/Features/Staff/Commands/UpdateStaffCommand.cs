using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.DTOs.Staff;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Staff.Commands;

public record UpdateStaffCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Bio,
    string? Role,
    string? Skills) : IRequest<StaffDto>;

public class UpdateStaffCommandHandler(
    IStaffRepository staffRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdateStaffCommand, StaffDto>
{
    public async Task<StaffDto> Handle(UpdateStaffCommand request, CancellationToken ct)
    {
        var staff = await staffRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Staff), request.Id);

        staff.Update(request.FirstName, request.LastName, request.Email,
            request.Phone, request.Bio, request.Role, request.Skills);
        await staffRepository.UpdateAsync(staff, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<StaffDto>(staff);
    }
}
