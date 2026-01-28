using IngApp.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class CommissionRuleConfiguration : IEntityTypeConfiguration<CommissionRule>
{
    public void Configure(EntityTypeBuilder<CommissionRule> builder)
    {
        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(cr => cr.Code)
            .IsUnique();

        builder.Property(cr => cr.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cr => cr.Description)
            .HasMaxLength(1000);

        builder.Property(cr => cr.CommissionPercentage)
            .HasPrecision(5, 2) // مثلاً 99.99%
            .IsRequired();

        builder.Property(cr => cr.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(cr => cr.CreatedAt)
            .IsRequired();

        // Seed Data: قوانین پورسانت اولیه
        builder.HasData(
            new CommissionRule
            {
                Id = 1,
                Code = "UnlockContactCommission",
                Title = "پورسانت باز کردن اطلاعات تماس",
                Description = "پورسانت از هزینه باز کردن اطلاعات تماس آگهی",
                CommissionPercentage = 10.00m, // 10%
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CommissionRule
            {
                Id = 2,
                Code = "SubscriptionCommission",
                Title = "پورسانت خرید اشتراک",
                Description = "پورسانت از خرید اشتراک",
                CommissionPercentage = 15.00m, // 15%
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}










