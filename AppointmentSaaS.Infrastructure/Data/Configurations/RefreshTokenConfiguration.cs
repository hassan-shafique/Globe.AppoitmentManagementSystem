using AppointmentSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSaaS.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Token).IsRequired().HasMaxLength(500);
        builder.HasIndex(r => r.Token).IsUnique();
        builder.Property(r => r.ReplacedByToken).HasMaxLength(500);
        builder.Property(r => r.RevokedReason).HasMaxLength(300);

        builder.HasOne(r => r.AppUser).WithMany(u => u.RefreshTokens).HasForeignKey(r => r.AppUserId).OnDelete(DeleteBehavior.Cascade);
    }
}
