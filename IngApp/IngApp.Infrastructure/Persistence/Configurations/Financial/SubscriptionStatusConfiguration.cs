using IngApp.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class SubscriptionStatusConfiguration : IEntityTypeConfiguration<SubscriptionStatus>
{
    public void Configure(EntityTypeBuilder<SubscriptionStatus> builder)
    {
        builder.HasKey(ss => ss.Id);

        builder.Property(ss => ss.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(ss => ss.Code)
            .IsUnique();

        builder.Property(ss => ss.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ss => ss.Description)
            .HasMaxLength(500);

        builder.Property(ss => ss.IsActive)
            .IsRequired();

        // Seed Data
        builder.HasData(
            new SubscriptionStatus { Id = 1, Code = "Active", Title = "فعال", Description = "اشتراک فعال است", IsActive = true },
            new SubscriptionStatus { Id = 2, Code = "Expired", Title = "منقضی شده", Description = "اشتراک منقضی شده است", IsActive = true },
            new SubscriptionStatus { Id = 3, Code = "Cancelled", Title = "لغو شده", Description = "اشتراک لغو شده است", IsActive = true },
            new SubscriptionStatus { Id = 4, Code = "Pending", Title = "در انتظار", Description = "اشتراک در انتظار فعال‌سازی", IsActive = true }
        );
    }
}










