using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.DTOs.Services;
using AppointmentSaaS.Application.Features.Services.Queries;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using FluentAssertions;
using Moq;

namespace AppointmentSaaS.Tests.Application.Services;

public class ServiceQueryHandlerTests
{
    private readonly Mock<IServiceRepository> _repoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private static Service MakeService(string name = "Haircut") =>
        Service.Create(TenantId, name, null, 30, 25.00m, 10);

    private static ServiceDto ToDto(Service s) =>
        new(s.Id, s.TenantId, s.BusinessId, s.Name, s.Description,
            s.DurationMinutes, s.Price, s.BufferTimeMinutes, s.IsActive, s.CreatedAt, s.UpdatedAt);

    [Fact]
    public async Task GetServiceById_WhenExists_ShouldReturnDto()
    {
        var s = MakeService();
        _repoMock.Setup(r => r.GetByIdAsync(s.Id, It.IsAny<CancellationToken>())).ReturnsAsync(s);
        _mapperMock.Setup(m => m.Map<ServiceDto>(s)).Returns(ToDto(s));

        var handler = new GetServiceByIdQueryHandler(_repoMock.Object, _mapperMock.Object);
        var result = await handler.Handle(new GetServiceByIdQuery(s.Id), CancellationToken.None);

        result.Id.Should().Be(s.Id);
        result.Name.Should().Be("Haircut");
    }

    [Fact]
    public async Task GetServiceById_WhenNotFound_ShouldThrowNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        var handler = new GetServiceByIdQueryHandler(_repoMock.Object, _mapperMock.Object);
        var act = async () => await handler.Handle(new GetServiceByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetServicesByTenant_ShouldReturnMappedList()
    {
        var services = new List<Service> { MakeService("A"), MakeService("B") };
        var dtos = services.Select(ToDto).ToList();

        _repoMock.Setup(r => r.GetByTenantAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services.AsReadOnly());
        _mapperMock.Setup(m => m.Map<IReadOnlyList<ServiceDto>>(services.AsReadOnly()))
            .Returns(dtos.AsReadOnly());

        var handler = new GetServicesByTenantQueryHandler(_repoMock.Object, _mapperMock.Object);
        var result = await handler.Handle(new GetServicesByTenantQuery(TenantId), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveServicesByTenant_ShouldReturnOnlyActive()
    {
        var active = MakeService("Active");
        var dtos = new List<ServiceDto> { ToDto(active) };

        _repoMock.Setup(r => r.GetActiveByTenantAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service> { active }.AsReadOnly());
        _mapperMock.Setup(m => m.Map<IReadOnlyList<ServiceDto>>(It.IsAny<IReadOnlyList<Service>>()))
            .Returns(dtos.AsReadOnly());

        var handler = new GetActiveServicesByTenantQueryHandler(_repoMock.Object, _mapperMock.Object);
        var result = await handler.Handle(new GetActiveServicesByTenantQuery(TenantId), CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetServicesByBusiness_ShouldReturnServicesForBusiness()
    {
        var businessId = Guid.NewGuid();
        var services = new List<Service> { MakeService("Service A") };
        var dtos = services.Select(ToDto).ToList();

        _repoMock.Setup(r => r.GetByBusinessAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services.AsReadOnly());
        _mapperMock.Setup(m => m.Map<IReadOnlyList<ServiceDto>>(services.AsReadOnly()))
            .Returns(dtos.AsReadOnly());

        var handler = new GetServicesByBusinessQueryHandler(_repoMock.Object, _mapperMock.Object);
        var result = await handler.Handle(new GetServicesByBusinessQuery(businessId), CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
