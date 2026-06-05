using AppointmentSaaS.Domain.Entities;
using FluentAssertions;

namespace AppointmentSaaS.Tests.Domain;

public class StaffTests
{
    private static Staff CreateStaff(
        string firstName = "Jane",
        string lastName = "Doe",
        string email = "jane@example.com")
        => Staff.Create(Guid.NewGuid(), string.Empty, firstName, lastName, email,
            phone: "+1234567890", role: "Doctor", skills: "Botox,Fillers");

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var s = CreateStaff();
        s.FirstName.Should().Be("Jane");
        s.LastName.Should().Be("Doe");
        s.Email.Should().Be("jane@example.com");
        s.Phone.Should().Be("+1234567890");
        s.Role.Should().Be("Doctor");
        s.Skills.Should().Be("Botox,Fillers");
        s.IsActive.Should().BeTrue();
        s.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_WithNullRoleAndSkills_ShouldSucceed()
    {
        var s = Staff.Create(Guid.NewGuid(), string.Empty, "John", "Smith", "john@example.com");
        s.Role.Should().BeNull();
        s.Skills.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyFirstName_ShouldThrow(string? firstName)
    {
        var act = () => Staff.Create(Guid.NewGuid(), string.Empty, firstName!, "Doe", "j@example.com");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_WithEmptyLastName_ShouldThrow(string? lastName)
    {
        var act = () => Staff.Create(Guid.NewGuid(), string.Empty, "Jane", lastName!, "j@example.com");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_WithEmptyEmail_ShouldThrow(string? email)
    {
        var act = () => Staff.Create(Guid.NewGuid(), string.Empty, "Jane", "Doe", email!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FullName_ShouldCombineFirstAndLastName()
    {
        var s = CreateStaff();
        s.FullName.Should().Be("Jane Doe");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateAllFields()
    {
        var s = CreateStaff();
        s.Update("Alice", "Smith", "alice@example.com", "+9876543210", "Senior doctor", "Consultant", "Surgery,Botox");

        s.FirstName.Should().Be("Alice");
        s.LastName.Should().Be("Smith");
        s.Email.Should().Be("alice@example.com");
        s.Phone.Should().Be("+9876543210");
        s.Bio.Should().Be("Senior doctor");
        s.Role.Should().Be("Consultant");
        s.Skills.Should().Be("Surgery,Botox");
        s.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_ClearingRoleAndSkills_ShouldSetNulls()
    {
        var s = CreateStaff();
        s.Update("Jane", "Doe", "jane@example.com", null, null, null, null);

        s.Role.Should().BeNull();
        s.Skills.Should().BeNull();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetIsActiveFalse()
    {
        var s = CreateStaff();
        s.Deactivate();
        s.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetIsActiveTrue()
    {
        var s = CreateStaff();
        s.Deactivate();
        s.Activate();
        s.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedTrue()
    {
        var s = CreateStaff();
        s.SoftDelete();
        s.IsDeleted.Should().BeTrue();
        s.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Restore_AfterSoftDelete_ShouldSetIsDeletedFalse()
    {
        var s = CreateStaff();
        s.SoftDelete();
        s.Restore();
        s.IsDeleted.Should().BeFalse();
        s.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void AssignToBusiness_ShouldUpdateBusinessId()
    {
        var s = CreateStaff();
        var businessId = Guid.NewGuid();
        s.AssignToBusiness(businessId);
        s.BusinessId.Should().Be(businessId);
    }

    [Fact]
    public void AssignToBusiness_WithNull_ShouldClearBusinessId()
    {
        var s = CreateStaff();
        s.AssignToBusiness(Guid.NewGuid());
        s.AssignToBusiness(null);
        s.BusinessId.Should().BeNull();
    }
}
