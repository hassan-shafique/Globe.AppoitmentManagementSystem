using AppointmentSaaS.Domain.Entities;
using FluentAssertions;

namespace AppointmentSaaS.Tests.Domain;

public class TenantTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var tenant = Tenant.Create("Acme Corp", "acme", "admin@acme.com");
        tenant.Name.Should().Be("Acme Corp");
        tenant.Slug.Should().Be("acme");
        tenant.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_SlugShouldBeLowerCase()
    {
        var tenant = Tenant.Create("Test Corp", "TestCORP", "test@test.com");
        tenant.Slug.Should().Be("testcorp");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrow()
    {
        var act = () => Tenant.Create("", "slug", "email@test.com");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var tenant = Tenant.Create("Test", "test", "test@test.com");
        tenant.Deactivate();
        tenant.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_AfterDeactivate_ShouldRestoreIsActiveTrue()
    {
        var tenant = Tenant.Create("Test", "test", "test@test.com");
        tenant.Deactivate();
        tenant.Activate();
        tenant.IsActive.Should().BeTrue();
    }
}
