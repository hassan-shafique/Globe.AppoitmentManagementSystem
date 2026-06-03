using AppointmentSaaS.Domain.Entities;
using FluentAssertions;

namespace AppointmentSaaS.Tests.Domain;

public class RefreshTokenTests
{
    [Fact]
    public void Create_WithFutureExpiry_ShouldBeActive()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "token123", DateTime.UtcNow.AddDays(7));
        token.IsActive.Should().BeTrue();
        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void Create_WithPastExpiry_ShouldBeInactive()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "token123", DateTime.UtcNow.AddDays(-1));
        token.IsActive.Should().BeFalse();
        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void Revoke_ShouldMarkTokenInactive()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "token123", DateTime.UtcNow.AddDays(7));
        token.Revoke("Logged out");
        token.IsActive.Should().BeFalse();
        token.IsRevoked.Should().BeTrue();
        token.RevokedReason.Should().Be("Logged out");
    }

    [Fact]
    public void Revoke_WithReplacedByToken_ShouldSetReplacedByToken()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "old-token", DateTime.UtcNow.AddDays(7));
        token.Revoke("Replaced", "new-token");
        token.ReplacedByToken.Should().Be("new-token");
    }
}
