using AppointmentSaaS.Application.Features.Staff.Commands;
using FluentValidation;

namespace AppointmentSaaS.Application.Features.Staff.Validators;

public class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
{
    public UpdateStaffCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Staff ID is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(30).When(x => x.Phone != null).WithMessage("Phone must not exceed 30 characters.");

        RuleFor(x => x.Role)
            .MaximumLength(100).When(x => x.Role != null).WithMessage("Role must not exceed 100 characters.");

        RuleFor(x => x.Skills)
            .MaximumLength(500).When(x => x.Skills != null).WithMessage("Skills must not exceed 500 characters.");
    }
}
