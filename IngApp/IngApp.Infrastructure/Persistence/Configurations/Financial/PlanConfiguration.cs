using IngApp.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.DurationMonths)
            .IsRequired();

        builder.Property(p => p.PriceRial)
            .IsRequired();

        builder.Property(p => p.UnlimitedContactViews)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // Seed Data: پلن‌های اولیه (1, 3, 6, 12 ماه)
        // قیمت‌ها به ریال هستند (مثلاً 100,000 تومان = 1,000,000 ریال)
        builder.HasData(
            new Plan
            {
                Id = 1,
                Code = "Plan1Month",
                Title = "پلن 1 ماهه",
                Description = "اشتراک 1 ماهه با دسترسی نامحدود به اطلاعات تماس",
                DurationMonths = 1,
                PriceRial = 1000000, // 100,000 تومان
                UnlimitedContactViews = true,
                IsActive = true,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new Plan
            {
                Id = 2,
                Code = "Plan3Month",
                Title = "پلن 3 ماهه",
                Description = "اشتراک 3 ماهه با دسترسی نامحدود به اطلاعات تماس",
                DurationMonths = 3,
                PriceRial = 2700000, // 270,000 تومان (10% تخفیف)
                UnlimitedContactViews = true,
                IsActive = true,
                DisplayOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new Plan
            {
                Id = 3,
                Code = "Plan6Month",
                Title = "پلن 6 ماهه",
                Description = "اشتراک 6 ماهه با دسترسی نامحدود به اطلاعات تماس",
                DurationMonths = 6,
                PriceRial = 5100000, // 510,000 تومان (15% تخفیف)
                UnlimitedContactViews = true,
                IsActive = true,
                DisplayOrder = 3,
                CreatedAt = DateTime.UtcNow
            },
            new Plan
            {
                Id = 4,
                Code = "Plan12Month",
                Title = "پلن 12 ماهه",
                Description = "اشتراک 12 ماهه با دسترسی نامحدود به اطلاعات تماس",
                DurationMonths = 12,
                PriceRial = 9600000, // 960,000 تومان (20% تخفیف)
                UnlimitedContactViews = true,
                IsActive = true,
                DisplayOrder = 4,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}











