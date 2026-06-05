using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Services;
using AppointmentSaaS.Application.Features.Services.Commands;
using AppointmentSaaS.Application.Features.Services.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppointmentSaaS.Web.Pages.Services;

[Authorize]
public class IndexModel(IMediator mediator, ICurrentUserService currentUserService) : PageModel
{
    public IReadOnlyList<ServiceDto> Services { get; private set; } = [];

    [TempData] public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (currentUserService.TenantId is not Guid tenantId)
            return RedirectToPage("/Auth/Login");

        Services = await mediator.Send(new GetServicesByTenantQuery(tenantId), ct);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteServiceCommand(id), ct);
        SuccessMessage = "Service deleted successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostActivateAsync(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ActivateServiceCommand(id), ct);
        SuccessMessage = "Service activated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeactivateServiceCommand(id), ct);
        SuccessMessage = "Service deactivated.";
        return RedirectToPage();
    }
}
