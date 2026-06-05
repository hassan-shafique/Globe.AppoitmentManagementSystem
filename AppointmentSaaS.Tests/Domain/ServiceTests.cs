using AppointmentSaaS.Domain.Entities;
using FluentAssertions;

namespace AppointmentSaaS.Tests.Domain;

public class ServiceTests
{
    private static Service CreateService(
        string name = "Haircut",
        int duration = 30,
        decimal price = 25.00m,
        int bufferTime = 10)
        => Service.Create(Guid.NewGuid(), name, "A test service", duration, price, bufferTime);

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var s = CreateService();
        s.Name.Should().Be("Haircut");
        s.Description.Should().Be("A test service");
        s.DurationMinutes.Should().Be(30);
        s.Price.Should().Be(25.00m);
        s.BufferTimeMinutes.Should().Be(10);
        s.IsActive.Should().BeTrue();
        s.IsDeleted.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ShouldThrow(string? name)
    {
        var act = () => Service.Create(Guid.NewGuid(), name!, null, 30, 10.00m);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveDuration_ShouldThrow(int duration)
    {
        var act = () => Service.Create(Guid.NewGuid(), "Cut", null, duration, 10.00m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldThrow()
    {
        var act = () => Service.Create(Guid.NewGuid(), "Cut", null, 30, -1.00m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativeBufferTime_ShouldThrow()
    {
        var act = () => Service.Create(Guid.NewGuid(), "Cut", null, 30, 10.00m, -5);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithZeroBufferTime_ShouldSucceed()
    {
        var s = Service.Create(Guid.NewGuid(), "Cut", null, 30, 10.00m, 0);
        s.BufferTimeMinutes.Should().Be(0);
    }

    [Fact]
    public void Create_WithZeroPrice_ShouldSucceed()
    {
        var s = Service.Create(Guid.NewGuid(), "Free Service", null, 15, 0m);
        s.Price.Should().Be(0m);
    }

    [Fact]
    public void Create_WithBusinessId_ShouldSetBusinessId()
    {
        var businessId = Guid.NewGuid();
        var s = Service.Create(Guid.NewGuid(), "Cut", null, 30, 10.00m, 0, businessId);
        s.BusinessId.Should().Be(businessId);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetIsActiveFalse()
    {
        var s = CreateService();
        s.Deactivate();
        s.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetIsActiveTrue()
    {
        var s = CreateService();
        s.Deactivate();
        s.Activate();
        s.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateProperties()
    {
        var s = CreateService();
        s.Update("Full Treatment", "Updated desc", 60, 50.00m, 15);

        s.Name.Should().Be("Full Treatment");
        s.Description.Should().Be("Updated desc");
        s.DurationMinutes.Should().Be(60);
        s.Price.Should().Be(50.00m);
        s.BufferTimeMinutes.Should().Be(15);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Update_WithEmptyName_ShouldThrow(string? name)
    {
        var s = CreateService();
        var act = () => s.Update(name!, null, 30, 10.00m, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedTrue()
    {
        var s = CreateService();
        s.SoftDelete();
        s.IsDeleted.Should().BeTrue();
        s.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Restore_AfterSoftDelete_ShouldSetIsDeletedFalse()
    {
        var s = CreateService();
        s.SoftDelete();
        s.Restore();
        s.IsDeleted.Should().BeFalse();
        s.DeletedAt.Should().BeNull();
    }
}
