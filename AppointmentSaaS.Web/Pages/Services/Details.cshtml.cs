using AppointmentSaaS.Application.DTOs.Services;
using AppointmentSaaS.Application.Features.Services.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppointmentSaaS.Web.Pages.Services;

[Authorize]
public class DetailsModel(IMediator mediator) : PageModel
{
    public ServiceDto Service { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        Service = await mediator.Send(new GetServiceByIdQuery(id), ct);
        return Page();
    }
}
