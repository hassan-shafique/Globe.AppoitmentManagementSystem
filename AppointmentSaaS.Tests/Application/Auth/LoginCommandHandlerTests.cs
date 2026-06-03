using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.Features.Auth.Commands;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AppointmentSaaS.Tests.Application.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<IRepository<AppUser>> _appUserRepoMock = new();
    private readonly Mock<IRepository<RefreshToken>> _refreshTokenRepoMock = new();
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private LoginCommandHandler CreateHandler() => new(
        _identityServiceMock.Object,
        _jwtServiceMock.Object,
        _appUserRepoMock.Object,
        _refreshTokenRepoMock.Object,
        _tenantRepoMock.Object,
        _unitOfWorkMock.Object);

    [Fact]
    public async Task Handle_WithInvalidTenantSlug_ShouldThrowNotFoundException()
    {
        _tenantRepoMock.Setup(r => r.GetBySlugAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var handler = CreateHandler();
        var command = new LoginCommand("user@test.com", "Password1!", "nonexistent");

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithInvalidCredentials_ShouldThrowValidationException()
    {
        var tenant = Tenant.Create("Test Corp", "test", "admin@test.com");

        _tenantRepoMock.Setup(r => r.GetBySlugAsync("test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        _identityServiceMock.Setup(m => m.ValidateCredentialsAsync("user@test.com", "WrongPassword!"))
            .ReturnsAsync((false, (string?)null, (IList<string>)[], "Invalid credentials."));

        var handler = CreateHandler();
        var command = new LoginCommand("user@test.com", "WrongPassword!", "test");

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
