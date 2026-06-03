using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.Features.Auth.Commands;
using FluentAssertions;
using Moq;

namespace AppointmentSaaS.Tests.Application.Auth;

public class VerifyEmailCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();

    private VerifyEmailCommandHandler CreateHandler() => new(_identityServiceMock.Object);

    [Fact]
    public async Task Handle_WithValidToken_ShouldConfirmEmail()
    {
        _identityServiceMock.Setup(s => s.ConfirmEmailAsync("user@test.com", "valid-token"))
            .ReturnsAsync((true, (string?)null));

        var handler = CreateHandler();
        var command = new VerifyEmailCommand("user@test.com", "valid-token");

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ShouldThrowValidationException()
    {
        _identityServiceMock.Setup(s => s.ConfirmEmailAsync("user@test.com", "bad-token"))
            .ReturnsAsync((false, "Invalid token."));

        var handler = CreateHandler();
        var command = new VerifyEmailCommand("user@test.com", "bad-token");

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ShouldThrowValidationException()
    {
        _identityServiceMock.Setup(s => s.ConfirmEmailAsync("user@test.com", "expired-token"))
            .ReturnsAsync((false, "Token has expired."));

        var handler = CreateHandler();
        var command = new VerifyEmailCommand("user@test.com", "expired-token");

        var act = async () => await handler.Handle(command, CancellationToken.None);
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().ContainKey("Token")
            .WhoseValue.Should().Contain(v => v.Contains("Token has expired"));
    }
}
