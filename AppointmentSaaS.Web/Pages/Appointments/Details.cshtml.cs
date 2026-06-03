using AppointmentSaaS.Application.DTOs.Appointments;
using AppointmentSaaS.Application.Features.Appointments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppointmentSaaS.Web.Pages.Appointments;

[Authorize]
public class DetailsModel(IMediator mediator) : PageModel
{
    public AppointmentDto? Appointment { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        try
        {
            Appointment = await mediator.Send(new GetAppointmentByIdQuery(id), ct);
            return Page();
        }
        catch (Application.Common.Exceptions.NotFoundException)
        {
            return NotFound();
        }
    }
}
