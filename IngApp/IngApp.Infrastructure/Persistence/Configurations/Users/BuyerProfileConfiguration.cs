using IngApp.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Users;

public class BuyerProfileConfiguration : IEntityTypeConfiguration<BuyerProfile>
{
    public void Configure(EntityTypeBuilder<BuyerProfile> builder)
    {
        builder.HasKey(bp => bp.Id);

        builder.Property(bp => bp.UserId)
            .IsRequired();

        builder.HasIndex(bp => bp.UserId)
            .IsUnique(); // هر کاربر فقط یک BuyerProfile می‌تواند داشته باشد

        builder.Property(bp => bp.BusinessName)
            .HasMaxLength(200);

        builder.Property(bp => bp.ContactMobile)
            .HasMaxLength(20);

        builder.Property(bp => bp.Province)
            .HasMaxLength(100);

        builder.Property(bp => bp.City)
            .HasMaxLength(100);

        builder.Property(bp => bp.Address)
            .HasMaxLength(500);

        builder.Property(bp => bp.Description)
            .HasMaxLength(1000);

        builder.Property(bp => bp.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(bp => bp.User)
            .WithOne(u => u.BuyerProfile)
            .HasForeignKey<BuyerProfile>(bp => bp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bp => bp.ReferredByVisitor)
            .WithMany()
            .HasForeignKey(bp => bp.ReferredByVisitorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index
        builder.HasIndex(bp => bp.ReferredByVisitorId);
    }
}











