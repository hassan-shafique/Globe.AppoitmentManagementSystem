using AppointmentSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSaaS.Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Notes).HasMaxLength(1000);
        builder.Property(a => a.CancellationReason).HasMaxLength(500);
        builder.Property(a => a.Status).IsRequired();

        builder.HasOne(a => a.Tenant).WithMany(t => t.Appointments).HasForeignKey(a => a.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Service).WithMany(s => s.Appointments).HasForeignKey(a => a.ServiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Staff).WithMany(s => s.Appointments).HasForeignKey(a => a.StaffId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Client).WithMany().HasForeignKey(a => a.ClientId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Customer).WithMany(c => c.Appointments).HasForeignKey(a => a.CustomerId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Business).WithMany(b => b.Appointments).HasForeignKey(a => a.BusinessId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.TenantId, a.StartTime });
        builder.HasIndex(a => new { a.StaffId, a.StartTime, a.EndTime });
    }
}
