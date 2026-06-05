using AppointmentSaaS.Application.Features.Services.Commands;
using AppointmentSaaS.Application.Features.Services.Validators;
using FluentAssertions;

namespace AppointmentSaaS.Tests.Application.Services;

public class ServiceValidatorTests
{
    private readonly CreateServiceCommandValidator _createValidator = new();
    private readonly UpdateServiceCommandValidator _updateValidator = new();

    [Fact]
    public void CreateValidator_WithValidCommand_ShouldPass()
    {
        var command = new CreateServiceCommand("Haircut", "A haircut", 30, 25.00m, 10, null);
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateValidator_WithEmptyName_ShouldFail(string name)
    {
        var command = new CreateServiceCommand(name, null, 30, 25.00m, 0, null);
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public void CreateValidator_WithNameTooLong_ShouldFail()
    {
        var command = new CreateServiceCommand(new string('A', 201), null, 30, 25.00m, 0, null);
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void CreateValidator_WithNonPositiveDuration_ShouldFail(int duration)
    {
        var command = new CreateServiceCommand("Cut", null, duration, 25.00m, 0, null);
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.DurationMinutes));
    }

    [Fact]
    public void CreateValidator_WithNegativePrice_ShouldFail()
    {
        var command = new CreateServiceCommand("Cut", null, 30, -1.00m, 0, null);
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Price));
    }

    [Fact]
    public void CreateValidator_WithNegativeBufferTime_ShouldFail()
    {
        var command = new CreateServiceCommand("Cut", null, 30, 25.00m, -1, null);
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.BufferTimeMinutes));
    }

    [Fact]
    public void UpdateValidator_WithEmptyId_ShouldFail()
    {
        var command = new UpdateServiceCommand(Guid.Empty, "Cut", null, 30, 25.00m, 0);
        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Id));
    }

    [Fact]
    public void UpdateValidator_WithValidCommand_ShouldPass()
    {
        var command = new UpdateServiceCommand(Guid.NewGuid(), "Full Treatment", "Desc", 60, 50.00m, 15);
        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateValidator_WithZeroPriceAndZeroBuffer_ShouldPass()
    {
        var command = new CreateServiceCommand("Free Service", null, 15, 0m, 0, null);
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
