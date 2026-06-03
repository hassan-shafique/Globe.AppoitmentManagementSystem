using AppointmentSaaS.Application.DTOs.Tenants;
using AppointmentSaaS.Application.Features.Tenants.Commands;
using AppointmentSaaS.Application.Features.Tenants.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSaaS.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class TenantsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTenantBySlugQuery(slug), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateTenantCommand(request.Name, request.Slug, request.ContactEmail), ct);
        return CreatedAtAction(nameof(GetBySlug), new { slug = result.Slug }, result);
    }
}
