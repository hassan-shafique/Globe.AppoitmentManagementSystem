using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Appointments;
using AppointmentSaaS.Application.Features.Appointments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppointmentSaaS.Web.Pages.Appointments;

[Authorize]
public class IndexModel(IMediator mediator, ICurrentUserService currentUserService) : PageModel
{
    public IReadOnlyList<AppointmentDto> Appointments { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (currentUserService.TenantId is not Guid tenantId)
            return RedirectToPage("/Auth/Login");
        Appointments = await mediator.Send(new GetAppointmentsByTenantQuery(tenantId), ct);
        return Page();
    }
}
