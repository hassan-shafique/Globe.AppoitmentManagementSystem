using AppointmentSaaS.Application.Features.Appointments.Commands;
using AppointmentSaaS.Application.Features.Services.Queries;
using AppointmentSaaS.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSaaS.Web.Pages.Appointments;

[Authorize]
public class CreateModel(IMediator mediator, ICurrentUserService currentUserService) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public List<SelectListItem> ServiceOptions { get; set; } = [];
    public List<SelectListItem> StaffOptions { get; set; } = [];

    public class InputModel
    {
        [Required] public Guid ServiceId { get; set; }
        [Required] public Guid StaffId { get; set; }
        [Required] public DateTime StartTime { get; set; } = DateTime.Now.AddHours(1);
        public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        await LoadOptionsAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync(ct);
            return Page();
        }
        try
        {
            await mediator.Send(new CreateAppointmentCommand(Input.ServiceId, Input.StaffId, Input.StartTime, Input.Notes), ct);
            return RedirectToPage("Index");
        }
        catch (Application.Common.Exceptions.ValidationException ex)
        {
            foreach (var (key, msgs) in ex.Errors)
                foreach (var msg in msgs)
                    ModelState.AddModelError(key, msg);
            await LoadOptionsAsync(ct);
            return Page();
        }
        catch (Domain.Exceptions.AppointmentConflictException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadOptionsAsync(ct);
            return Page();
        }
    }

    private async Task LoadOptionsAsync(CancellationToken ct)
    {
        if (currentUserService.TenantId is Guid tenantId)
        {
            var services = await mediator.Send(new GetServicesByTenantQuery(tenantId), ct);
            ServiceOptions = services.Select(s => new SelectListItem($"{s.Name} ({s.DurationMinutes} min - ${s.Price})", s.Id.ToString())).ToList();
        }
        // Staff options would come from a staff query - placeholder for now
        StaffOptions = [];
    }
}
