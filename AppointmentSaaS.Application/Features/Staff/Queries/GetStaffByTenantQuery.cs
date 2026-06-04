using AppointmentSaaS.Application.DTOs.Staff;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Staff.Queries;

public record GetStaffByTenantQuery(Guid TenantId) : IRequest<IReadOnlyList<StaffDto>>;

public class GetStaffByTenantQueryHandler(
    IRepository<Domain.Entities.Staff> staffRepository,
    IMapper mapper)
    : IRequestHandler<GetStaffByTenantQuery, IReadOnlyList<StaffDto>>
{
    public async Task<IReadOnlyList<StaffDto>> Handle(GetStaffByTenantQuery request, CancellationToken ct)
    {
        var staff = await staffRepository.FindAsync(s => s.TenantId == request.TenantId && s.IsActive, ct);
        return mapper.Map<IReadOnlyList<StaffDto>>(staff);
    }
}
