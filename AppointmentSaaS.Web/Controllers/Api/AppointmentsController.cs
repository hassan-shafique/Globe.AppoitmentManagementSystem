using AppointmentSaaS.Application.DTOs.Appointments;
using AppointmentSaaS.Application.Features.Appointments.Commands;
using AppointmentSaaS.Application.Features.Appointments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSaaS.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController(IMediator mediator) : ControllerBase
{
    [HttpGet("tenant/{tenantId:guid}")]
    public async Task<IActionResult> GetByTenant(Guid tenantId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAppointmentsByTenantQuery(tenantId), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAppointmentByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateAppointmentCommand(request.ServiceId, request.StaffId, request.StartTime, request.Notes), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/confirm")]
    [Authorize(Roles = "TenantAdmin,Staff")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ConfirmAppointmentCommand(id), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] string reason, CancellationToken ct)
    {
        await mediator.Send(new CancelAppointmentCommand(id, reason), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/complete")]
    [Authorize(Roles = "TenantAdmin,Staff")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new CompleteAppointmentCommand(id), ct);
        return NoContent();
    }
}
