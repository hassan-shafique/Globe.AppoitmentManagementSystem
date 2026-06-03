using AppointmentSaaS.Application.DTOs.Services;
using AppointmentSaaS.Application.Features.Services.Commands;
using AppointmentSaaS.Application.Features.Services.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSaaS.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ServicesController(IMediator mediator) : ControllerBase
{
    [HttpGet("tenant/{tenantId:guid}")]
    public async Task<IActionResult> GetByTenant(Guid tenantId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetServicesByTenantQuery(tenantId), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "TenantAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateServiceCommand(request.Name, request.Description, request.DurationMinutes, request.Price), ct);
        return CreatedAtAction(nameof(GetByTenant), new { tenantId = result.TenantId }, result);
    }
}
