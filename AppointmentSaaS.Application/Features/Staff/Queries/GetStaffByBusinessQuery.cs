using AppointmentSaaS.Application.DTOs.Staff;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Staff.Queries;

public record GetStaffByBusinessQuery(Guid BusinessId) : IRequest<IReadOnlyList<StaffDto>>;

public class GetStaffByBusinessQueryHandler(
    IStaffRepository staffRepository,
    IMapper mapper)
    : IRequestHandler<GetStaffByBusinessQuery, IReadOnlyList<StaffDto>>
{
    public async Task<IReadOnlyList<StaffDto>> Handle(GetStaffByBusinessQuery request, CancellationToken ct)
    {
        var staff = await staffRepository.GetByBusinessAsync(request.BusinessId, ct);
        return mapper.Map<IReadOnlyList<StaffDto>>(staff);
    }
}
