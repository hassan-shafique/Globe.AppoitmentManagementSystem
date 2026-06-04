using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Businesses;
using AppointmentSaaS.Application.Features.Businesses.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppointmentSaaS.Web.Pages.Businesses;

[Authorize]
public class IndexModel(IMediator mediator, ICurrentUserService currentUserService) : PageModel
{
    public IReadOnlyList<BusinessDto> Businesses { get; private set; } = [];

    [TempData] public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (currentUserService.TenantId is not Guid tenantId)
            return RedirectToPage("/Auth/Login");

        Businesses = await mediator.Send(new GetBusinessesByTenantQuery(tenantId), ct);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken ct)
    {
        await mediator.Send(new Application.Features.Businesses.Commands.DeleteBusinessCommand(id), ct);
        SuccessMessage = "Business deleted successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostActivateAsync(Guid id, CancellationToken ct)
    {
        await mediator.Send(new Application.Features.Businesses.Commands.ActivateBusinessCommand(id), ct);
        SuccessMessage = "Business activated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid id, CancellationToken ct)
    {
        await mediator.Send(new Application.Features.Businesses.Commands.DeactivateBusinessCommand(id), ct);
        SuccessMessage = "Business deactivated.";
        return RedirectToPage();
    }
}
