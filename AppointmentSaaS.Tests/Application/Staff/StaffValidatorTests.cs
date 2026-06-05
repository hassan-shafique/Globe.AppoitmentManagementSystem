using AppointmentSaaS.Application.Features.Staff.Commands;
using AppointmentSaaS.Application.Features.Staff.Validators;
using FluentAssertions;

namespace AppointmentSaaS.Tests.Application.Staff;

public class StaffValidatorTests
{
    private readonly CreateStaffCommandValidator _createValidator = new();
    private readonly UpdateStaffCommandValidator _updateValidator = new();

    private static CreateStaffCommand ValidCreate() =>
        new("Jane", "Doe", "jane@example.com", "+1234567890", null, "Doctor", "Botox,Fillers", null);

    private static UpdateStaffCommand ValidUpdate() =>
        new(Guid.NewGuid(), "Jane", "Doe", "jane@example.com", null, null, null, null);

    [Fact]
    public void CreateValidator_WithValidCommand_ShouldPass()
    {
        var result = _createValidator.Validate(ValidCreate());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateValidator_WithEmptyFirstName_ShouldFail(string firstName)
    {
        var command = ValidCreate() with { FirstName = firstName };
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.FirstName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateValidator_WithEmptyLastName_ShouldFail(string lastName)
    {
        var command = ValidCreate() with { LastName = lastName };
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.LastName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void CreateValidator_WithInvalidEmail_ShouldFail(string email)
    {
        var command = ValidCreate() with { Email = email };
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Email));
    }

    [Fact]
    public void CreateValidator_WithRoleTooLong_ShouldFail()
    {
        var command = ValidCreate() with { Role = new string('A', 101) };
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Role));
    }

    [Fact]
    public void CreateValidator_WithSkillsTooLong_ShouldFail()
    {
        var command = ValidCreate() with { Skills = new string('A', 501) };
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Skills));
    }

    [Fact]
    public void CreateValidator_WithNullRoleAndSkills_ShouldPass()
    {
        var command = ValidCreate() with { Role = null, Skills = null };
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateValidator_WithValidCommand_ShouldPass()
    {
        var result = _updateValidator.Validate(ValidUpdate());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateValidator_WithEmptyId_ShouldFail()
    {
        var command = ValidUpdate() with { Id = Guid.Empty };
        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Id));
    }

    [Fact]
    public void UpdateValidator_WithInvalidEmail_ShouldFail()
    {
        var command = ValidUpdate() with { Email = "bad-email" };
        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Email));
    }
}
