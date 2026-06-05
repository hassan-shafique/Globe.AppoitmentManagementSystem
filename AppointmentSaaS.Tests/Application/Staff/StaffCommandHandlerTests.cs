using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.DTOs.Staff;
using AppointmentSaaS.Application.Features.Staff.Commands;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using FluentAssertions;
using Moq;
using StaffEntity = AppointmentSaaS.Domain.Entities.Staff;

namespace AppointmentSaaS.Tests.Application.Staff;

public class StaffCommandHandlerTests
{
    private readonly Mock<IStaffRepository> _repoMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private static StaffEntity MakeStaff() =>
        StaffEntity.Create(TenantId, string.Empty, "Jane", "Doe", "jane@example.com",
            phone: "+1234567890", role: "Doctor", skills: "Botox");

    private static StaffDto MakeDto(StaffEntity s) =>
        new(s.Id, s.TenantId, s.BusinessId, s.FirstName, s.LastName, s.FullName,
            s.Email, s.Phone, s.Bio, s.Role, s.Skills, s.IsActive, s.CreatedAt, s.UpdatedAt);

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateStaff_WithValidTenant_ShouldReturnDto()
    {
        var staff = MakeStaff();
        _currentUserMock.Setup(c => c.TenantId).Returns(TenantId);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<StaffEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _mapperMock.Setup(m => m.Map<StaffDto>(It.IsAny<StaffEntity>())).Returns(MakeDto(staff));

        var handler = new CreateStaffCommandHandler(_repoMock.Object, _currentUserMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);
        var command = new CreateStaffCommand("Jane", "Doe", "jane@example.com", "+1234567890", null, "Doctor", "Botox", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.FirstName.Should().Be("Jane");
        result.Role.Should().Be("Doctor");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateStaff_WithoutTenantContext_ShouldThrowForbiddenException()
    {
        _currentUserMock.Setup(c => c.TenantId).Returns((Guid?)null);

        var handler = new CreateStaffCommandHandler(_repoMock.Object, _currentUserMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);
        var command = new CreateStaffCommand("Jane", "Doe", "jane@example.com", null, null, null, null, null);

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStaff_WhenExists_ShouldUpdateAndReturnDto()
    {
        var staff = MakeStaff();
        var updatedDto = MakeDto(staff) with { FirstName = "Alice", Role = "Consultant" };

        _repoMock.Setup(r => r.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>())).ReturnsAsync(staff);
        _mapperMock.Setup(m => m.Map<StaffDto>(It.IsAny<StaffEntity>())).Returns(updatedDto);

        var handler = new UpdateStaffCommandHandler(_repoMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);
        var command = new UpdateStaffCommand(staff.Id, "Alice", "Doe", "alice@example.com", null, null, "Consultant", "Surgery");

        var result = await handler.Handle(command, CancellationToken.None);

        result.FirstName.Should().Be("Alice");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStaff_WhenNotFound_ShouldThrowNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StaffEntity?)null);

        var handler = new UpdateStaffCommandHandler(_repoMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);
        var command = new UpdateStaffCommand(Guid.NewGuid(), "Jane", "Doe", "j@example.com", null, null, null, null);

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteStaff_WhenExists_ShouldSoftDelete()
    {
        var staff = MakeStaff();
        _repoMock.Setup(r => r.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>())).ReturnsAsync(staff);

        var handler = new DeleteStaffCommandHandler(_repoMock.Object, _unitOfWorkMock.Object);
        await handler.Handle(new DeleteStaffCommand(staff.Id), CancellationToken.None);

        staff.IsDeleted.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteStaff_WhenNotFound_ShouldThrowNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StaffEntity?)null);

        var handler = new DeleteStaffCommandHandler(_repoMock.Object, _unitOfWorkMock.Object);
        var act = async () => await handler.Handle(new DeleteStaffCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── Activate / Deactivate ─────────────────────────────────────────────────

    [Fact]
    public async Task DeactivateStaff_WhenExists_ShouldSetInactive()
    {
        var staff = MakeStaff();
        _repoMock.Setup(r => r.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>())).ReturnsAsync(staff);

        var handler = new DeactivateStaffCommandHandler(_repoMock.Object, _unitOfWorkMock.Object);
        await handler.Handle(new DeactivateStaffCommand(staff.Id), CancellationToken.None);

        staff.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ActivateStaff_WhenInactive_ShouldSetActive()
    {
        var staff = MakeStaff();
        staff.Deactivate();
        _repoMock.Setup(r => r.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>())).ReturnsAsync(staff);

        var handler = new ActivateStaffCommandHandler(_repoMock.Object, _unitOfWorkMock.Object);
        await handler.Handle(new ActivateStaffCommand(staff.Id), CancellationToken.None);

        staff.IsActive.Should().BeTrue();
    }
}
