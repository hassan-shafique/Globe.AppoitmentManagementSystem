using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Services;
using AppointmentSaaS.Application.Features.Services.Commands;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using FluentAssertions;
using Moq;

namespace AppointmentSaaS.Tests.Application.Services;

public class ServiceCommandHandlerTests
{
    private readonly Mock<IServiceRepository> _repoMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private static Service MakeService() =>
        Service.Create(TenantId, "Haircut", "A haircut", 30, 25.00m, 10);

    private static ServiceDto MakeDto(Service s) =>
        new(s.Id, s.TenantId, s.BusinessId, s.Name, s.Description,
            s.DurationMinutes, s.Price, s.BufferTimeMinutes, s.IsActive, s.CreatedAt, s.UpdatedAt);

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateService_WithValidTenant_ShouldReturnDto()
    {
        var service = MakeService();
        _currentUserMock.Setup(c => c.TenantId).Returns(TenantId);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _mapperMock.Setup(m => m.Map<ServiceDto>(It.IsAny<Service>())).Returns(MakeDto(service));

        var handler = new CreateServiceCommandHandler(_repoMock.Object, _currentUserMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);
        var command = new CreateServiceCommand("Haircut", "A haircut", 30, 25.00m, 10, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Haircut");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateService_WithoutTenantContext_ShouldThrowForbiddenException()
    {
        _currentUserMock.Setup(c => c.TenantId).Returns((Guid?)null);

        var handler = new CreateServiceCommandHandler(_repoMock.Object, _currentUserMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);
        var command = new CreateServiceCommand("Haircut", null, 30, 25.00m, 0, null);

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateService_WhenExists_ShouldUpdateAndReturnDto()
    {
        var service = MakeService();
        var updatedDto = MakeDto(service) with { Name = "Full Treatment", DurationMinutes = 60 };

        _repoMock.Setup(r => r.GetByIdAsync(service.Id, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _mapperMock.Setup(m => m.Map<ServiceDto>(It.IsAny<Service>())).Returns(updatedDto);

        var handler = new UpdateServiceCommandHandler(_repoMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);
        var command = new UpdateServiceCommand(service.Id, "Full Treatment", null, 60, 50.00m, 15);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Full Treatment");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateService_WhenNotFound_ShouldThrowNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        var handler = new UpdateServiceCommandHandler(_repoMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);
        var command = new UpdateServiceCommand(Guid.NewGuid(), "X", null, 30, 10.00m, 0);

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteService_WhenExists_ShouldSoftDelete()
    {
        var service = MakeService();
        _repoMock.Setup(r => r.GetByIdAsync(service.Id, It.IsAny<CancellationToken>())).ReturnsAsync(service);

        var handler = new DeleteServiceCommandHandler(_repoMock.Object, _unitOfWorkMock.Object);
        await handler.Handle(new DeleteServiceCommand(service.Id), CancellationToken.None);

        service.IsDeleted.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteService_WhenNotFound_ShouldThrowNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        var handler = new DeleteServiceCommandHandler(_repoMock.Object, _unitOfWorkMock.Object);
        var act = async () => await handler.Handle(new DeleteServiceCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── Activate / Deactivate ─────────────────────────────────────────────────

    [Fact]
    public async Task DeactivateService_WhenExists_ShouldSetInactive()
    {
        var service = MakeService();
        _repoMock.Setup(r => r.GetByIdAsync(service.Id, It.IsAny<CancellationToken>())).ReturnsAsync(service);

        var handler = new DeactivateServiceCommandHandler(_repoMock.Object, _unitOfWorkMock.Object);
        await handler.Handle(new DeactivateServiceCommand(service.Id), CancellationToken.None);

        service.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ActivateService_WhenInactive_ShouldSetActive()
    {
        var service = MakeService();
        service.Deactivate();
        _repoMock.Setup(r => r.GetByIdAsync(service.Id, It.IsAny<CancellationToken>())).ReturnsAsync(service);

        var handler = new ActivateServiceCommandHandler(_repoMock.Object, _unitOfWorkMock.Object);
        await handler.Handle(new ActivateServiceCommand(service.Id), CancellationToken.None);

        service.IsActive.Should().BeTrue();
    }
}
