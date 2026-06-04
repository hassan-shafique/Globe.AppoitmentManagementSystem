using AppointmentSaaS.Application.Features.Staff.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSaaS.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class StaffController(IMediator mediator) : ControllerBase
{
    [HttpGet("tenant/{tenantId:guid}")]
    public async Task<IActionResult> GetByTenant(Guid tenantId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetStaffByTenantQuery(tenantId), ct);
        return Ok(result);
    }
}
