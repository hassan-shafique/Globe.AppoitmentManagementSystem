using AppointmentSaaS.Application.Features.Businesses.Commands;
using FluentValidation;

namespace AppointmentSaaS.Application.Features.Businesses.Validators;

public class UpdateBusinessCommandValidator : AbstractValidator<UpdateBusinessCommand>
{
    public UpdateBusinessCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Business ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Business name is required.")
            .MaximumLength(200).WithMessage("Business name must not exceed 200 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid business type.");

        RuleFor(x => x.Address)
            .MaximumLength(500).When(x => x.Address != null).WithMessage("Address must not exceed 500 characters.");

        RuleFor(x => x.City)
            .MaximumLength(100).When(x => x.City != null).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(50).When(x => x.Phone != null).WithMessage("Phone must not exceed 50 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("A valid email address is required.")
            .MaximumLength(256).When(x => x.Email != null).WithMessage("Email must not exceed 256 characters.");
    }
}
