using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Infrastructure.Data;
using AppointmentSaaS.Infrastructure.Identity;
using AppointmentSaaS.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AppointmentSaaS.Tests.Infrastructure.Repositories;

public class AppointmentRepositoryTests : IDisposable
{
    private readonly IServiceScope _scope;
    private readonly AppDbContext _context;
    private readonly AppointmentRepository _repository;

    public AppointmentRepositoryTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(o =>
            o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>();
        services.AddLogging();

        _scope = services.BuildServiceProvider().CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _repository = new AppointmentRepository(_context);
    }

    [Fact]
    public async Task HasConflictAsync_WithOverlappingAppointment_ShouldReturnTrue()
    {
        var staffId = Guid.NewGuid();
        var baseTime = DateTime.UtcNow.Date.AddHours(10);

        var existing = Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), staffId, Guid.NewGuid(), baseTime, baseTime.AddHours(1));
        await _context.Appointments.AddAsync(existing);
        await _context.SaveChangesAsync();

        var hasConflict = await _repository.HasConflictAsync(staffId, baseTime.AddMinutes(30), baseTime.AddMinutes(90));
        hasConflict.Should().BeTrue();
    }

    [Fact]
    public async Task HasConflictAsync_WithNonOverlappingAppointment_ShouldReturnFalse()
    {
        var staffId = Guid.NewGuid();
        var baseTime = DateTime.UtcNow.Date.AddHours(10);

        var existing = Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), staffId, Guid.NewGuid(), baseTime, baseTime.AddHours(1));
        await _context.Appointments.AddAsync(existing);
        await _context.SaveChangesAsync();

        var hasConflict = await _repository.HasConflictAsync(staffId, baseTime.AddHours(2), baseTime.AddHours(3));
        hasConflict.Should().BeFalse();
    }

    [Fact]
    public async Task HasConflictAsync_WithCancelledAppointment_ShouldReturnFalse()
    {
        var staffId = Guid.NewGuid();
        var baseTime = DateTime.UtcNow.Date.AddHours(10);

        var existing = Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), staffId, Guid.NewGuid(), baseTime, baseTime.AddHours(1));
        existing.Cancel("Cancelled for test");
        await _context.Appointments.AddAsync(existing);
        await _context.SaveChangesAsync();

        var hasConflict = await _repository.HasConflictAsync(staffId, baseTime.AddMinutes(30), baseTime.AddMinutes(90));
        hasConflict.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
        _scope.Dispose();
    }
}
