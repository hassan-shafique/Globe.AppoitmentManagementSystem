using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.DTOs.Staff;
using AppointmentSaaS.Application.Features.Staff.Queries;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using FluentAssertions;
using Moq;
using StaffEntity = AppointmentSaaS.Domain.Entities.Staff;

namespace AppointmentSaaS.Tests.Application.Staff;

public class StaffQueryHandlerTests
{
    private readonly Mock<IStaffRepository> _repoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private static StaffEntity MakeStaff() =>
        StaffEntity.Create(TenantId, string.Empty, "Jane", "Doe", "jane@example.com");

    private static StaffDto MakeDto(StaffEntity s) =>
        new(s.Id, s.TenantId, s.BusinessId, s.FirstName, s.LastName, s.FullName,
            s.Email, s.Phone, s.Bio, s.Role, s.Skills, s.IsActive, s.CreatedAt, s.UpdatedAt);

    [Fact]
    public async Task GetStaffByTenant_ShouldReturnMappedList()
    {
        var staff = new List<StaffEntity> { MakeStaff() };
        _repoMock.Setup(r => r.GetByTenantAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _mapperMock.Setup(m => m.Map<IReadOnlyList<StaffDto>>(staff))
            .Returns(staff.Select(MakeDto).ToList());

        var handler = new GetStaffByTenantQueryHandler(_repoMock.Object, _mapperMock.Object);
        var result = await handler.Handle(new GetStaffByTenantQuery(TenantId), CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetActiveStaffByTenant_ShouldReturnOnlyActiveStaff()
    {
        var staff = new List<StaffEntity> { MakeStaff() };
        _repoMock.Setup(r => r.GetActiveByTenantAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _mapperMock.Setup(m => m.Map<IReadOnlyList<StaffDto>>(staff))
            .Returns(staff.Select(MakeDto).ToList());

        var handler = new GetActiveStaffByTenantQueryHandler(_repoMock.Object, _mapperMock.Object);
        var result = await handler.Handle(new GetActiveStaffByTenantQuery(TenantId), CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetStaffById_WhenExists_ShouldReturnDto()
    {
        var staff = MakeStaff();
        _repoMock.Setup(r => r.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>())).ReturnsAsync(staff);
        _mapperMock.Setup(m => m.Map<StaffDto>(staff)).Returns(MakeDto(staff));

        var handler = new GetStaffByIdQueryHandler(_repoMock.Object, _mapperMock.Object);
        var result = await handler.Handle(new GetStaffByIdQuery(staff.Id), CancellationToken.None);

        result.Id.Should().Be(staff.Id);
    }

    [Fact]
    public async Task GetStaffById_WhenNotFound_ShouldThrowNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StaffEntity?)null);

        var handler = new GetStaffByIdQueryHandler(_repoMock.Object, _mapperMock.Object);
        var act = async () => await handler.Handle(new GetStaffByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetStaffByBusiness_ShouldReturnMappedList()
    {
        var businessId = Guid.NewGuid();
        var staff = new List<StaffEntity> { MakeStaff() };
        _repoMock.Setup(r => r.GetByBusinessAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _mapperMock.Setup(m => m.Map<IReadOnlyList<StaffDto>>(staff))
            .Returns(staff.Select(MakeDto).ToList());

        var handler = new GetStaffByBusinessQueryHandler(_repoMock.Object, _mapperMock.Object);
        var result = await handler.Handle(new GetStaffByBusinessQuery(businessId), CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
