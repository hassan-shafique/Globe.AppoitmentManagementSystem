using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Enums;
using FluentAssertions;

namespace AppointmentSaaS.Tests.Domain;

public class BusinessTests
{
    private static Business CreateBusiness(string name = "Test Clinic", BusinessType type = BusinessType.Doctor)
        => Business.Create(Guid.NewGuid(), name, type, "123 Main St", "Cityville", "555-1234", "clinic@test.com");

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var b = CreateBusiness();
        b.Name.Should().Be("Test Clinic");
        b.Type.Should().Be(BusinessType.Doctor);
        b.Address.Should().Be("123 Main St");
        b.City.Should().Be("Cityville");
        b.Phone.Should().Be("555-1234");
        b.Email.Should().Be("clinic@test.com");
        b.IsActive.Should().BeTrue();
        b.IsDeleted.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ShouldThrow(string? name)
    {
        var act = () => Business.Create(Guid.NewGuid(), name!, BusinessType.Generic);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_DefaultType_ShouldBeGeneric()
    {
        var b = Business.Create(Guid.NewGuid(), "My Shop");
        b.Type.Should().Be(BusinessType.Generic);
    }

    [Theory]
    [InlineData(BusinessType.Doctor)]
    [InlineData(BusinessType.Dentist)]
    [InlineData(BusinessType.Teacher)]
    [InlineData(BusinessType.Tutor)]
    [InlineData(BusinessType.Salon)]
    [InlineData(BusinessType.Consultant)]
    [InlineData(BusinessType.Generic)]
    public void Create_WithEachBusinessType_ShouldSucceed(BusinessType type)
    {
        var b = Business.Create(Guid.NewGuid(), "Business", type);
        b.Type.Should().Be(type);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetIsActiveFalse()
    {
        var b = CreateBusiness();
        b.Deactivate();
        b.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetIsActiveTrue()
    {
        var b = CreateBusiness();
        b.Deactivate();
        b.Activate();
        b.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateProperties()
    {
        var b = CreateBusiness();
        b.Update("New Name", BusinessType.Salon, "456 Oak Ave", "Townsville", "555-9999", "new@test.com");

        b.Name.Should().Be("New Name");
        b.Type.Should().Be(BusinessType.Salon);
        b.Address.Should().Be("456 Oak Ave");
        b.City.Should().Be("Townsville");
        b.Phone.Should().Be("555-9999");
        b.Email.Should().Be("new@test.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Update_WithEmptyName_ShouldThrow(string? name)
    {
        var b = CreateBusiness();
        var act = () => b.Update(name!, BusinessType.Generic, null, null, null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_WithNullOptionalFields_ShouldSucceed()
    {
        var b = CreateBusiness();
        b.Update("Updated Name", BusinessType.Tutor, null, null, null, null);

        b.Name.Should().Be("Updated Name");
        b.Address.Should().BeNull();
        b.City.Should().BeNull();
        b.Phone.Should().BeNull();
        b.Email.Should().BeNull();
    }

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedTrue()
    {
        var b = CreateBusiness();
        b.SoftDelete();
        b.IsDeleted.Should().BeTrue();
        b.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Restore_AfterSoftDelete_ShouldSetIsDeletedFalse()
    {
        var b = CreateBusiness();
        b.SoftDelete();
        b.Restore();
        b.IsDeleted.Should().BeFalse();
        b.DeletedAt.Should().BeNull();
    }
}
