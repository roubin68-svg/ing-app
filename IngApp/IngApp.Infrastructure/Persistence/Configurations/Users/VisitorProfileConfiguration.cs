using IngApp.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Users;

public class VisitorProfileConfiguration : IEntityTypeConfiguration<VisitorProfile>
{
    public void Configure(EntityTypeBuilder<VisitorProfile> builder)
    {
        builder.HasKey(vp => vp.Id);

        builder.Property(vp => vp.UserId)
            .IsRequired();

        builder.Property(vp => vp.ReferralCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(vp => vp.ReferralCode)
            .IsUnique();

        builder.HasIndex(vp => vp.UserId)
            .IsUnique(); // هر کاربر فقط یک VisitorProfile می‌تواند داشته باشد

        builder.Property(vp => vp.BusinessName)
            .HasMaxLength(200);

        builder.Property(vp => vp.ContactMobile)
            .HasMaxLength(20);

        builder.Property(vp => vp.Province)
            .HasMaxLength(100);

        builder.Property(vp => vp.City)
            .HasMaxLength(100);

        builder.Property(vp => vp.Address)
            .HasMaxLength(500);

        builder.Property(vp => vp.Description)
            .HasMaxLength(1000);

        builder.Property(vp => vp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(vp => vp.CreatedAt)
            .IsRequired();

        // Relationship
        builder.HasOne(vp => vp.User)
            .WithOne(u => u.VisitorProfile)
            .HasForeignKey<VisitorProfile>(vp => vp.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}











