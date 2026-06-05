using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Staff;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Staff.Commands;

public record CreateStaffCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Bio,
    string? Role,
    string? Skills,
    Guid? BusinessId) : IRequest<StaffDto>;

public class CreateStaffCommandHandler(
    IStaffRepository staffRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateStaffCommand, StaffDto>
{
    public async Task<StaffDto> Handle(CreateStaffCommand request, CancellationToken ct)
    {
        var tenantId = currentUserService.TenantId
            ?? throw new ForbiddenException("Tenant context is required.");

        var staff = Domain.Entities.Staff.Create(
            tenantId, string.Empty,
            request.FirstName, request.LastName, request.Email,
            request.Phone, request.Role, request.Skills, request.BusinessId);

        await staffRepository.AddAsync(staff, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<StaffDto>(staff);
    }
}
