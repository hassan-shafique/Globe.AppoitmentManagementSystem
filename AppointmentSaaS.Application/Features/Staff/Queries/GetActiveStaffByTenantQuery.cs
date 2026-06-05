using AppointmentSaaS.Application.DTOs.Staff;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Staff.Queries;

public record GetActiveStaffByTenantQuery(Guid TenantId) : IRequest<IReadOnlyList<StaffDto>>;

public class GetActiveStaffByTenantQueryHandler(
    IStaffRepository staffRepository,
    IMapper mapper)
    : IRequestHandler<GetActiveStaffByTenantQuery, IReadOnlyList<StaffDto>>
{
    public async Task<IReadOnlyList<StaffDto>> Handle(GetActiveStaffByTenantQuery request, CancellationToken ct)
    {
        var staff = await staffRepository.GetActiveByTenantAsync(request.TenantId, ct);
        return mapper.Map<IReadOnlyList<StaffDto>>(staff);
    }
}
