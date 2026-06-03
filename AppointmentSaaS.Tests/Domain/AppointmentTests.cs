using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Enums;
using AppointmentSaaS.Domain.Events;
using AppointmentSaaS.Domain.Exceptions;
using FluentAssertions;

namespace AppointmentSaaS.Tests.Domain;

public class AppointmentTests
{
    private static Appointment CreateValidAppointment(
        DateTime? start = null,
        DateTime? end = null)
    {
        var s = start ?? DateTime.UtcNow.AddHours(1);
        var e = end ?? s.AddHours(1);
        return Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), s, e);
    }

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var apt = CreateValidAppointment();
        apt.Status.Should().Be(AppointmentStatus.Pending);
        apt.DomainEvents.Should().ContainSingle(e => e is AppointmentCreatedEvent);
    }

    [Fact]
    public void Create_WithEndBeforeStart_ShouldThrowDomainException()
    {
        var now = DateTime.UtcNow;
        var act = () => Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), now.AddHours(2), now.AddHours(1));
        act.Should().Throw<DomainException>().WithMessage("*End time*");
    }

    [Fact]
    public void Confirm_WhenPending_ShouldSetConfirmedStatus()
    {
        var apt = CreateValidAppointment();
        apt.Confirm();
        apt.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ShouldThrowDomainException()
    {
        var apt = CreateValidAppointment();
        apt.Confirm();
        var act = () => apt.Confirm();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_WhenPending_ShouldSetCancelledAndRaiseEvent()
    {
        var apt = CreateValidAppointment();
        apt.Cancel("Client requested");
        apt.Status.Should().Be(AppointmentStatus.Cancelled);
        apt.CancellationReason.Should().Be("Client requested");
        apt.DomainEvents.Should().Contain(e => e is AppointmentCancelledEvent);
    }

    [Fact]
    public void Cancel_WhenAlreadyCompleted_ShouldThrowDomainException()
    {
        var apt = CreateValidAppointment();
        apt.Confirm();
        apt.Complete();
        var act = () => apt.Cancel("Late cancellation");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Complete_WhenConfirmed_ShouldSetCompletedStatus()
    {
        var apt = CreateValidAppointment();
        apt.Confirm();
        apt.Complete();
        apt.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public void MarkNoShow_WhenConfirmed_ShouldSetNoShowStatus()
    {
        var apt = CreateValidAppointment();
        apt.Confirm();
        apt.MarkNoShow();
        apt.Status.Should().Be(AppointmentStatus.NoShow);
    }

    [Theory]
    [InlineData(0, 2, 1, 3, true)]   // overlap: starts before, ends during
    [InlineData(1, 3, 0, 2, true)]   // overlap: starts during, ends after
    [InlineData(0, 1, 1, 2, false)]  // adjacent: no overlap
    [InlineData(0, 1, 2, 3, false)]  // no overlap: before
    public void OverlapsWith_ShouldDetectCorrectly(int s1h, int e1h, int s2h, int e2h, bool expected)
    {
        var base_ = DateTime.UtcNow.Date.AddHours(8);
        var apt = Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            base_.AddHours(s1h), base_.AddHours(e1h));
        apt.OverlapsWith(base_.AddHours(s2h), base_.AddHours(e2h)).Should().Be(expected);
    }
}
