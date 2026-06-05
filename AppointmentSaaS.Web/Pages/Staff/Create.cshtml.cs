using AppointmentSaaS.Application.Features.Staff.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSaaS.Web.Pages.Staff;

[Authorize(Roles = "TenantAdmin,SuperAdmin")]
public class CreateModel(IMediator mediator) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Bio { get; set; }
        public string? Role { get; set; }
        public string? Skills { get; set; }
    }

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            await mediator.Send(new CreateStaffCommand(
                Input.FirstName, Input.LastName, Input.Email,
                Input.Phone, Input.Bio, Input.Role, Input.Skills, null), ct);
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
