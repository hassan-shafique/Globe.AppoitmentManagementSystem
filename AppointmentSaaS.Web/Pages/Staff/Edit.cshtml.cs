using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Features.Staff.Commands;
using AppointmentSaaS.Application.Features.Staff.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSaaS.Web.Pages.Staff;

[Authorize(Roles = "TenantAdmin,SuperAdmin")]
public class EditModel(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)] public Guid StaffId { get; set; }
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

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var staff = await mediator.Send(new GetStaffByIdQuery(StaffId), ct);
        Input = new InputModel
        {
            FirstName = staff.FirstName,
            LastName = staff.LastName,
            Email = staff.Email,
            Phone = staff.Phone,
            Bio = staff.Bio,
            Role = staff.Role,
            Skills = staff.Skills
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            await mediator.Send(new UpdateStaffCommand(
                StaffId, Input.FirstName, Input.LastName, Input.Email,
                Input.Phone, Input.Bio, Input.Role, Input.Skills), ct);
            return RedirectToPage("Index");
        }
        catch (AppointmentSaaS.Application.Common.Exceptions.ValidationException ex)
        {
            foreach (var (key, msgs) in ex.Errors)
                foreach (var msg in msgs)
                    ModelState.AddModelError(key, msg);
            return Page();
        }
    }
}
