using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Businesses;
using AppointmentSaaS.Application.Features.Businesses.Commands;
using AppointmentSaaS.Application.Features.Businesses.Queries;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Enums;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using FluentAssertions;
using Moq;

namespace AppointmentSaaS.Tests.Application.Businesses;

public class BusinessCommandHandlerTests
{
    private readonly Mock<IBusinessRepository> _repoMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private CreateBusinessCommandHandler CreateHandler() =>
        new(_repoMock.Object, _currentUserMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);

    private static Business MakeBusiness() =>
        Business.Create(TenantId, "Test Clinic", BusinessType.Doctor, "123 Main", "City", "555", "a@b.com");

    private static BusinessDto MakeDto(Business b) =>
        new(b.Id, b.TenantId, b.Name, b.Type, b.Address, b.City, b.Phone, b.Email, b.IsActive, b.CreatedAt, b.UpdatedAt);

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBusiness_WithValidTenant_ShouldReturnDto()
    {
        var business = MakeBusiness();
        _currentUserMock.Setup(c => c.TenantId).Returns(TenantId);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Business>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _mapperMock.Setup(m => m.Map<BusinessDto>(It.IsAny<Business>())).Returns(MakeDto(business));

        var handler = CreateHandler();
        var command = new CreateBusinessCommand("Test Clinic", BusinessType.Doctor, "123 Main", "City", "555", "a@b.com");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Test Clinic");
        result.Type.Should().Be(BusinessType.Doctor);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBusiness_WithoutTenantContext_ShouldThrowForbiddenException()
    {
        _currentUserMock.Setup(c => c.TenantId).Returns((Guid?)null);

        var handler = CreateHandler();
        var command = new CreateBusinessCommand("Clinic", BusinessType.Generic, null, null, null, null);

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateBusiness_WhenExists_ShouldUpdateAndReturnDto()
    {
        var business = MakeBusiness();
        var updatedDto = new BusinessDto(business.Id, business.TenantId, "Updated", BusinessType.Salon,
            null, null, null, null, true, business.CreatedAt, DateTime.UtcNow);

        _repoMock.Setup(r => r.GetByIdAsync(business.Id, It.IsAny<CancellationToken>())).ReturnsAsync(business);
        _mapperMock.Setup(m => m.Map<BusinessDto>(It.IsAny<Business>())).Returns(updatedDto);

        var handler = new UpdateBusinessCommandHandler(_repoMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);
        var command = new UpdateBusinessCommand(business.Id, "Updated", BusinessType.Salon, null, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Updated");
        result.Type.Should().Be(BusinessType.Salon);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBusiness_WhenNotFound_ShouldThrowNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        var handler = new UpdateBusinessCommandHandler(_repoMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);
        var command = new UpdateBusinessCommand(Guid.NewGuid(), "X", BusinessType.Generic, null, null, null, null);

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteBusiness_WhenExists_ShouldSoftDelete()
    {
        var business = MakeBusiness();
        _repoMock.Setup(r => r.GetByIdAsync(business.Id, It.IsAny<CancellationToken>())).ReturnsAsync(business);

        var handler = new DeleteBusinessCommandHandler(_repoMock.Object, _unitOfWorkMock.Object);
        await handler.Handle(new DeleteBusinessCommand(business.Id), CancellationToken.None);

        business.IsDeleted.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteBusiness_WhenNotFound_ShouldThrowNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        var handler = new DeleteBusinessCommandHandler(_repoMock.Object, _unitOfWorkMock.Object);
        var act = async () => await handler.Handle(new DeleteBusinessCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── Activate / Deactivate ─────────────────────────────────────────────────

    [Fact]
    public async Task DeactivateBusiness_WhenExists_ShouldSetInactive()
    {
        var business = MakeBusiness();
        _repoMock.Setup(r => r.GetByIdAsync(business.Id, It.IsAny<CancellationToken>())).ReturnsAsync(business);

        var handler = new DeactivateBusinessCommandHandler(_repoMock.Object, _unitOfWorkMock.Object);
        await handler.Handle(new DeactivateBusinessCommand(business.Id), CancellationToken.None);

        business.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ActivateBusiness_WhenInactive_ShouldSetActive()
    {
        var business = MakeBusiness();
        business.Deactivate();
        _repoMock.Setup(r => r.GetByIdAsync(business.Id, It.IsAny<CancellationToken>())).ReturnsAsync(business);

        var handler = new ActivateBusinessCommandHandler(_repoMock.Object, _unitOfWorkMock.Object);
        await handler.Handle(new ActivateBusinessCommand(business.Id), CancellationToken.None);

        business.IsActive.Should().BeTrue();
    }
}
