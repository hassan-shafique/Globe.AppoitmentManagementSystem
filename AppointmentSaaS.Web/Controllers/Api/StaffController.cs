using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Staff;
using AppointmentSaaS.Application.Features.Staff.Commands;
using AppointmentSaaS.Application.Features.Staff.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSaaS.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StaffController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenantId = currentUserService.TenantId;
        if (tenantId is null) return Forbid();

        var result = await mediator.Send(new GetStaffByTenantQuery(tenantId.Value), ct);
        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken ct)
    {
        var tenantId = currentUserService.TenantId;
        if (tenantId is null) return Forbid();

        var result = await mediator.Send(new GetActiveStaffByTenantQuery(tenantId.Value), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetStaffByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpGet("business/{businessId:guid}")]
    public async Task<IActionResult> GetByBusiness(Guid businessId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetStaffByBusinessQuery(businessId), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateStaffRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateStaffCommand(request.FirstName, request.LastName, request.Email,
                request.Phone, request.Bio, request.Role, request.Skills, request.BusinessId), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStaffRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateStaffCommand(id, request.FirstName, request.LastName, request.Email,
                request.Phone, request.Bio, request.Role, request.Skills), ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteStaffCommand(id), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ActivateStaffCommand(id), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeactivateStaffCommand(id), ct);
        return NoContent();
    }
}
