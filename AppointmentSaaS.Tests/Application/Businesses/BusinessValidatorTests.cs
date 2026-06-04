using AppointmentSaaS.Application.Features.Businesses.Commands;
using AppointmentSaaS.Application.Features.Businesses.Validators;
using AppointmentSaaS.Domain.Enums;
using FluentAssertions;

namespace AppointmentSaaS.Tests.Application.Businesses;

public class BusinessValidatorTests
{
    private readonly CreateBusinessCommandValidator _createValidator = new();
    private readonly UpdateBusinessCommandValidator _updateValidator = new();

    [Fact]
    public void CreateValidator_WithValidCommand_ShouldPass()
    {
        var command = new CreateBusinessCommand("Clinic", BusinessType.Doctor, null, null, null, null);
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateValidator_WithEmptyName_ShouldFail(string name)
    {
        var command = new CreateBusinessCommand(name, BusinessType.Doctor, null, null, null, null);
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public void CreateValidator_WithInvalidEmail_ShouldFail()
    {
        var command = new CreateBusinessCommand("Clinic", BusinessType.Doctor, null, null, null, "not-an-email");
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Email));
    }

    [Fact]
    public void CreateValidator_WithNameTooLong_ShouldFail()
    {
        var longName = new string('A', 201);
        var command = new CreateBusinessCommand(longName, BusinessType.Generic, null, null, null, null);
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateValidator_WithEmptyId_ShouldFail()
    {
        var command = new UpdateBusinessCommand(Guid.Empty, "Clinic", BusinessType.Generic, null, null, null, null);
        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Id));
    }

    [Fact]
    public void UpdateValidator_WithValidCommand_ShouldPass()
    {
        var command = new UpdateBusinessCommand(Guid.NewGuid(), "Salon", BusinessType.Salon, "Addr", "City", "555", "a@b.com");
        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
