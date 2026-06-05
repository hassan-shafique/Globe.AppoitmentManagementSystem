using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Staff;
using AppointmentSaaS.Application.Features.Staff.Commands;
using AppointmentSaaS.Application.Features.Staff.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppointmentSaaS.Web.Pages.Staff;

[Authorize]
public class IndexModel(IMediator mediator, ICurrentUserService currentUserService) : PageModel
{
    public IReadOnlyList<StaffDto> StaffMembers { get; private set; } = [];

    [TempData] public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (currentUserService.TenantId is not Guid tenantId)
            return RedirectToPage("/Auth/Login");

        StaffMembers = await mediator.Send(new GetStaffByTenantQuery(tenantId), ct);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteStaffCommand(id), ct);
        SuccessMessage = "Staff member deleted successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostActivateAsync(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ActivateStaffCommand(id), ct);
        SuccessMessage = "Staff member activated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeactivateStaffCommand(id), ct);
        SuccessMessage = "Staff member deactivated.";
        return RedirectToPage();
    }
}
