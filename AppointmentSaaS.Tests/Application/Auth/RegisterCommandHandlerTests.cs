using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.Features.Auth.Commands;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AppointmentSaaS.Tests.Application.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IRepository<AppUser>> _appUserRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private RegisterCommandHandler CreateHandler() => new(
        _identityServiceMock.Object,
        _emailServiceMock.Object,
        _tenantRepoMock.Object,
        _appUserRepoMock.Object,
        _unitOfWorkMock.Object);

    [Fact]
    public async Task Handle_WithValidInput_ShouldCreateUserAndSendVerificationEmail()
    {
        var tenant = Tenant.Create("Test Corp", "test", "admin@test.com");
        var identityUserId = Guid.NewGuid().ToString();

        _tenantRepoMock.Setup(r => r.GetBySlugAsync("test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        _identityServiceMock.Setup(s => s.CreateUserAsync("user@test.com", "Password1!", "John", "Doe"))
            .ReturnsAsync((true, identityUserId, (string?)null));
        _identityServiceMock.Setup(s => s.GetRolesAsync(identityUserId))
            .ReturnsAsync(["Client"]);
        _identityServiceMock.Setup(s => s.GenerateEmailVerificationTokenAsync("user@test.com"))
            .ReturnsAsync((true, "verification-token", (string?)null));
        _appUserRepoMock.Setup(r => r.AddAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppUser entity, CancellationToken _) => entity);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var command = new RegisterCommand("John", "Doe", "user@test.com", "Password1!", "test");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Email.Should().Be("user@test.com");
        result.Message.Should().Contain("verify your account");

        _emailServiceMock.Verify(e => e.SendEmailVerificationAsync(
            "user@test.com", It.IsAny<string>(), "verification-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidTenantSlug_ShouldThrowNotFoundException()
    {
        _tenantRepoMock.Setup(r => r.GetBySlugAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var handler = CreateHandler();
        var command = new RegisterCommand("John", "Doe", "user@test.com", "Password1!", "nonexistent");

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenIdentityFails_ShouldThrowValidationException()
    {
        var tenant = Tenant.Create("Test Corp", "test", "admin@test.com");

        _tenantRepoMock.Setup(r => r.GetBySlugAsync("test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        _identityServiceMock.Setup(s => s.CreateUserAsync("user@test.com", "weak", "John", "Doe"))
            .ReturnsAsync((false, (string?)null, "Password too weak."));

        var handler = CreateHandler();
        var command = new RegisterCommand("John", "Doe", "user@test.com", "weak", "test");

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenVerificationTokenNull_ShouldNotSendVerificationEmail()
    {
        var tenant = Tenant.Create("Test Corp", "test", "admin@test.com");
        var identityUserId = Guid.NewGuid().ToString();

        _tenantRepoMock.Setup(r => r.GetBySlugAsync("test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        _identityServiceMock.Setup(s => s.CreateUserAsync("user@test.com", "Password1!", "John", "Doe"))
            .ReturnsAsync((true, identityUserId, (string?)null));
        _identityServiceMock.Setup(s => s.GetRolesAsync(identityUserId))
            .ReturnsAsync(["Client"]);
        _identityServiceMock.Setup(s => s.GenerateEmailVerificationTokenAsync("user@test.com"))
            .ReturnsAsync((false, (string?)null, "Error"));
        _appUserRepoMock.Setup(r => r.AddAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppUser entity, CancellationToken _) => entity);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var command = new RegisterCommand("John", "Doe", "user@test.com", "Password1!", "test");

        await handler.Handle(command, CancellationToken.None);

        _emailServiceMock.Verify(e => e.SendEmailVerificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
