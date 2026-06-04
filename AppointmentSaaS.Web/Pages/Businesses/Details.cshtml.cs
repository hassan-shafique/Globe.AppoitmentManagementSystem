using AppointmentSaaS.Application.DTOs.Businesses;
using AppointmentSaaS.Application.Features.Businesses.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppointmentSaaS.Web.Pages.Businesses;

[Authorize]
public class DetailsModel(IMediator mediator) : PageModel
{
    public BusinessDto Business { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        Business = await mediator.Send(new GetBusinessByIdQuery(id), ct);
        return Page();
    }
}
