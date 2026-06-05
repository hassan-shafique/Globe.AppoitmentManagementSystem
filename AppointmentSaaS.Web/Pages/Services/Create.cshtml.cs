using AppointmentSaaS.Application.Features.Services.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSaaS.Web.Pages.Services;

[Authorize(Roles = "TenantAdmin,SuperAdmin")]
public class CreateModel(IMediator mediator) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required] public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Duration must be at least 1 minute.")]
        public int DurationMinutes { get; set; } = 30;
        [Required, Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
        public decimal Price { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Buffer time cannot be negative.")]
        public int BufferTimeMinutes { get; set; }
    }

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            await mediator.Send(new CreateServiceCommand(
                Input.Name, Input.Description, Input.DurationMinutes,
                Input.Price, Input.BufferTimeMinutes, null), ct);
            return RedirectToPage("Index");
        }
        catch (Application.Common.Exceptions.ValidationException ex)
        {
            foreach (var (key, msgs) in ex.Errors)
                foreach (var msg in msgs)
                    ModelState.AddModelError(key, msg);
            return Page();
        }
    }
}
