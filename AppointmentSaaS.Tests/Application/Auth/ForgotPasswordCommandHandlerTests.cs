using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.Features.Auth.Commands;
using FluentAssertions;
using Moq;

namespace AppointmentSaaS.Tests.Application.Auth;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();

    private ForgotPasswordCommandHandler CreateHandler() => new(
        _identityServiceMock.Object,
        _emailServiceMock.Object);

    [Fact]
    public async Task Handle_WhenUserExists_ShouldSendPasswordResetEmail()
    {
        _identityServiceMock.Setup(s => s.GeneratePasswordResetTokenAsync("user@test.com"))
            .ReturnsAsync((true, "reset-token", (string?)null));

        var handler = CreateHandler();
        var command = new ForgotPasswordCommand("user@test.com");

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().NotThrowAsync();

        _emailServiceMock.Verify(e => e.SendPasswordResetAsync(
            "user@test.com", "user@test.com", "reset-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldNotSendEmail()
    {
        _identityServiceMock.Setup(s => s.GeneratePasswordResetTokenAsync("nonexistent@test.com"))
            .ReturnsAsync((true, (string?)null, (string?)null));

        var handler = CreateHandler();
        var command = new ForgotPasswordCommand("nonexistent@test.com");

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().NotThrowAsync();

        _emailServiceMock.Verify(e => e.SendPasswordResetAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
