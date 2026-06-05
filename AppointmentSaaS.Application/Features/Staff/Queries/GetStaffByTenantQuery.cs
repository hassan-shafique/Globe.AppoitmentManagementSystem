using AppointmentSaaS.Application.DTOs.Staff;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Staff.Queries;

public record GetStaffByTenantQuery(Guid TenantId) : IRequest<IReadOnlyList<StaffDto>>;

public class GetStaffByTenantQueryHandler(
    IStaffRepository staffRepository,
    IMapper mapper)
    : IRequestHandler<GetStaffByTenantQuery, IReadOnlyList<StaffDto>>
{
    public async Task<IReadOnlyList<StaffDto>> Handle(GetStaffByTenantQuery request, CancellationToken ct)
    {
        var staff = await staffRepository.GetByTenantAsync(request.TenantId, ct);
        return mapper.Map<IReadOnlyList<StaffDto>>(staff);
    }
}
