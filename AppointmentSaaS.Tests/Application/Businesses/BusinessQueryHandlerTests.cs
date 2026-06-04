using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.DTOs.Businesses;
using AppointmentSaaS.Application.Features.Businesses.Queries;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Enums;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using FluentAssertions;
using Moq;

namespace AppointmentSaaS.Tests.Application.Businesses;

public class BusinessQueryHandlerTests
{
    private readonly Mock<IBusinessRepository> _repoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private static Business MakeBusiness(string name = "Clinic") =>
        Business.Create(TenantId, name, BusinessType.Doctor);

    private static BusinessDto ToDto(Business b) =>
        new(b.Id, b.TenantId, b.Name, b.Type, b.Address, b.City, b.Phone, b.Email, b.IsActive, b.CreatedAt, b.UpdatedAt);

    [Fact]
    public async Task GetBusinessById_WhenExists_ShouldReturnDto()
    {
        var b = MakeBusiness();
        _repoMock.Setup(r => r.GetByIdAsync(b.Id, It.IsAny<CancellationToken>())).ReturnsAsync(b);
        _mapperMock.Setup(m => m.Map<BusinessDto>(b)).Returns(ToDto(b));

        var handler = new GetBusinessByIdQueryHandler(_repoMock.Object, _mapperMock.Object);
        var result = await handler.Handle(new GetBusinessByIdQuery(b.Id), CancellationToken.None);

        result.Id.Should().Be(b.Id);
        result.Name.Should().Be("Clinic");
    }

    [Fact]
    public async Task GetBusinessById_WhenNotFound_ShouldThrowNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        var handler = new GetBusinessByIdQueryHandler(_repoMock.Object, _mapperMock.Object);
        var act = async () => await handler.Handle(new GetBusinessByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetBusinessesByTenant_ShouldReturnMappedList()
    {
        var businesses = new List<Business> { MakeBusiness("A"), MakeBusiness("B") };
        var dtos = businesses.Select(ToDto).ToList();

        _repoMock.Setup(r => r.GetByTenantAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(businesses.AsReadOnly());
        _mapperMock.Setup(m => m.Map<IReadOnlyList<BusinessDto>>(businesses.AsReadOnly()))
            .Returns(dtos.AsReadOnly());

        var handler = new GetBusinessesByTenantQueryHandler(_repoMock.Object, _mapperMock.Object);
        var result = await handler.Handle(new GetBusinessesByTenantQuery(TenantId), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveBusinessesByTenant_ShouldReturnOnlyActive()
    {
        var active = MakeBusiness("Active");
        var dtos = new List<BusinessDto> { ToDto(active) };

        _repoMock.Setup(r => r.GetActiveByTenantAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Business> { active }.AsReadOnly());
        _mapperMock.Setup(m => m.Map<IReadOnlyList<BusinessDto>>(It.IsAny<IReadOnlyList<Business>>()))
            .Returns(dtos.AsReadOnly());

        var handler = new GetActiveBusinessesByTenantQueryHandler(_repoMock.Object, _mapperMock.Object);
        var result = await handler.Handle(new GetActiveBusinessesByTenantQuery(TenantId), CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
