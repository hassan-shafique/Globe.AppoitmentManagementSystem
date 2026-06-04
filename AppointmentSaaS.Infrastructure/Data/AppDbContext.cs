using AppointmentSaaS.Application.Common.Interfaces;
using AppointmentSaaS.Domain.Common;
using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Events;
using AppointmentSaaS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AppointmentSaaS.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Ignore<DomainEvent>();
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Soft-delete + tenant isolation filters.
        // tenantProvider.TenantId == null bypasses the tenant filter (super-admin / background jobs).
        builder.Entity<AppUser>().HasQueryFilter(e =>
            !e.IsDeleted && (tenantProvider.TenantId == null || e.TenantId == tenantProvider.TenantId));

        builder.Entity<Business>().HasQueryFilter(e =>
            !e.IsDeleted && (tenantProvider.TenantId == null || e.TenantId == tenantProvider.TenantId));

        builder.Entity<Staff>().HasQueryFilter(e =>
            !e.IsDeleted && (tenantProvider.TenantId == null || e.TenantId == tenantProvider.TenantId));

        builder.Entity<Service>().HasQueryFilter(e =>
            !e.IsDeleted && (tenantProvider.TenantId == null || e.TenantId == tenantProvider.TenantId));

        builder.Entity<Customer>().HasQueryFilter(e =>
            !e.IsDeleted && (tenantProvider.TenantId == null || e.TenantId == tenantProvider.TenantId));

        // Appointment uses AuditableEntity (no soft-delete), tenant filter only
        builder.Entity<Appointment>().HasQueryFilter(e =>
            tenantProvider.TenantId == null || e.TenantId == tenantProvider.TenantId);

        // Tenant table: soft-delete only — never filtered by TenantId (it IS the tenant)
        builder.Entity<Tenant>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return await base.SaveChangesAsync(ct);
    }
}
