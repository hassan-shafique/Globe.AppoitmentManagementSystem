using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.Features.Auth.Commands;
using FluentAssertions;
using Moq;

namespace AppointmentSaaS.Tests.Application.Auth;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();

    private ResetPasswordCommandHandler CreateHandler() => new(_identityServiceMock.Object);

    [Fact]
    public async Task Handle_WithValidToken_ShouldResetPassword()
    {
        _identityServiceMock.Setup(s => s.ResetPasswordAsync("user@test.com", "valid-token", "NewPassword1!"))
            .ReturnsAsync((true, (string?)null));

        var handler = CreateHandler();
        var command = new ResetPasswordCommand("user@test.com", "valid-token", "NewPassword1!", "NewPassword1!");

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WhenPasswordsDoNotMatch_ShouldThrowValidationException()
    {
        var handler = CreateHandler();
        var command = new ResetPasswordCommand("user@test.com", "token", "NewPassword1!", "Different1!");

        var act = async () => await handler.Handle(command, CancellationToken.None);
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().ContainKey("ConfirmPassword")
            .WhoseValue.Should().Contain(v => v.Contains("Passwords do not match"));
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ShouldThrowValidationException()
    {
        _identityServiceMock.Setup(s => s.ResetPasswordAsync("user@test.com", "bad-token", "NewPassword1!"))
            .ReturnsAsync((false, "Invalid token."));

        var handler = CreateHandler();
        var command = new ResetPasswordCommand("user@test.com", "bad-token", "NewPassword1!", "NewPassword1!");

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
