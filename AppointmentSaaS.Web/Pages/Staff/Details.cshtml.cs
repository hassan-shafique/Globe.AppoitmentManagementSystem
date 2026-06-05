using AppointmentSaaS.Application.DTOs.Staff;
using AppointmentSaaS.Application.Features.Staff.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppointmentSaaS.Web.Pages.Staff;

[Authorize]
public class DetailsModel(IMediator mediator) : PageModel
{
    public StaffDto Staff { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        Staff = await mediator.Send(new GetStaffByIdQuery(id), ct);
        return Page();
    }
}
