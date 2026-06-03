using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Appointments;
using AppointmentSaaS.Application.Features.Appointments.Queries;
using AppointmentSaaS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppointmentSaaS.Web.Pages.Dashboard;

[Authorize]
public class IndexModel(IMediator mediator, ICurrentUserService currentUserService) : PageModel
{
    public IReadOnlyList<AppointmentDto> RecentAppointments { get; private set; } = [];
    public int TotalAppointments { get; private set; }
    public int ConfirmedCount { get; private set; }
    public int PendingCount { get; private set; }
    public int CancelledCount { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (currentUserService.TenantId is not Guid tenantId)
            return RedirectToPage("/Auth/Login");

        var all = await mediator.Send(new GetAppointmentsByTenantQuery(tenantId), ct);
        TotalAppointments = all.Count;
        ConfirmedCount = all.Count(a => a.Status == AppointmentStatus.Confirmed);
        PendingCount = all.Count(a => a.Status == AppointmentStatus.Pending);
        CancelledCount = all.Count(a => a.Status == AppointmentStatus.Cancelled);
        RecentAppointments = all.Take(10).ToList();
        return Page();
    }
}
