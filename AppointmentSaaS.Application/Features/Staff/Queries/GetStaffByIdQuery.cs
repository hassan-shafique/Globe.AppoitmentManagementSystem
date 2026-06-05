using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.DTOs.Staff;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Staff.Queries;

public record GetStaffByIdQuery(Guid Id) : IRequest<StaffDto>;

public class GetStaffByIdQueryHandler(
    IStaffRepository staffRepository,
    IMapper mapper)
    : IRequestHandler<GetStaffByIdQuery, StaffDto>
{
    public async Task<StaffDto> Handle(GetStaffByIdQuery request, CancellationToken ct)
    {
        var staff = await staffRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Staff), request.Id);

        return mapper.Map<StaffDto>(staff);
    }
}
