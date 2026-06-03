using AppointmentSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSaaS.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(100);
        builder.HasIndex(t => t.Slug).IsUnique();
        builder.Property(t => t.ContactEmail).HasMaxLength(256);
        builder.Property(t => t.ContactPhone).HasMaxLength(50);
        builder.Property(t => t.LogoUrl).HasMaxLength(500);

        builder.HasOne(t => t.SubscriptionPlan)
            .WithMany(p => p.Tenants)
            .HasForeignKey(t => t.SubscriptionPlanId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
