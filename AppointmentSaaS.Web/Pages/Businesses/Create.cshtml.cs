using AppointmentSaaS.Application.Features.Businesses.Commands;
using AppointmentSaaS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSaaS.Web.Pages.Businesses;

[Authorize(Roles = "TenantAdmin,SuperAdmin")]
public class CreateModel(IMediator mediator) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required] public string Name { get; set; } = string.Empty;
        [Required] public BusinessType Type { get; set; } = BusinessType.Generic;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        [EmailAddress] public string? Email { get; set; }
    }

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            await mediator.Send(new CreateBusinessCommand(
                Input.Name, Input.Type, Input.Address, Input.City, Input.Phone, Input.Email), ct);
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
