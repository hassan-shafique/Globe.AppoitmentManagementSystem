using AppointmentSaaS.Application.Features.Services.Commands;
using FluentValidation;

namespace AppointmentSaaS.Application.Features.Services.Validators;

public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Service name is required.")
            .MaximumLength(200).WithMessage("Service name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).When(x => x.Description != null).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than 0 minutes.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");

        RuleFor(x => x.BufferTimeMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("Buffer time cannot be negative.");
    }
}
