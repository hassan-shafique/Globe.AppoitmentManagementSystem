using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Services;
using AppointmentSaaS.Application.Features.Services.Commands;
using AppointmentSaaS.Application.Features.Services.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSaaS.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicesController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenantId = currentUserService.TenantId;
        if (tenantId is null) return Forbid();

        var result = await mediator.Send(new GetServicesByTenantQuery(tenantId.Value), ct);
        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken ct)
    {
        var tenantId = currentUserService.TenantId;
        if (tenantId is null) return Forbid();

        var result = await mediator.Send(new GetActiveServicesByTenantQuery(tenantId.Value), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetServiceByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpGet("business/{businessId:guid}")]
    public async Task<IActionResult> GetByBusiness(Guid businessId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetServicesByBusinessQuery(businessId), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateServiceCommand(request.Name, request.Description, request.DurationMinutes,
                request.Price, request.BufferTimeMinutes, request.BusinessId), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateServiceCommand(id, request.Name, request.Description, request.DurationMinutes,
                request.Price, request.BufferTimeMinutes), ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteServiceCommand(id), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ActivateServiceCommand(id), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeactivateServiceCommand(id), ct);
        return NoContent();
    }
}
