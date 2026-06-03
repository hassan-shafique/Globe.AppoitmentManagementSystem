using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Application.Features.Appointments.Commands;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Exceptions;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using FluentAssertions;
using Moq;

namespace AppointmentSaaS.Tests.Application.Appointments;

public class CreateAppointmentCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepoMock = new();
    private readonly Mock<IRepository<Service>> _serviceRepoMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private CreateAppointmentCommandHandler CreateHandler() => new(
        _appointmentRepoMock.Object,
        _serviceRepoMock.Object,
        _currentUserMock.Object,
        _unitOfWorkMock.Object,
        _mapperMock.Object);

    [Fact]
    public async Task Handle_WithConflict_ShouldThrowAppointmentConflictException()
    {
        var tenantId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(1);

        var service = Service.Create(tenantId, "Haircut", null, 60, 50m);

        _serviceRepoMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _currentUserMock.Setup(c => c.TenantId).Returns(tenantId);
        _currentUserMock.Setup(c => c.UserId).Returns(Guid.NewGuid().ToString());
        _appointmentRepoMock.Setup(r => r.HasConflictAsync(staffId, startTime, startTime.AddMinutes(60), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler();
        var command = new CreateAppointmentCommand(serviceId, staffId, startTime, null);

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<AppointmentConflictException>();
    }

    [Fact]
    public async Task Handle_WithoutTenantContext_ShouldThrowForbiddenException()
    {
        var serviceId = Guid.NewGuid();
        var service = Service.Create(Guid.NewGuid(), "Haircut", null, 60, 50m);

        _serviceRepoMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _currentUserMock.Setup(c => c.TenantId).Returns((Guid?)null);

        var handler = CreateHandler();
        var command = new CreateAppointmentCommand(serviceId, Guid.NewGuid(), DateTime.UtcNow.AddHours(1), null);

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
