using IngApp.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class PricingConfiguration : IEntityTypeConfiguration<Pricing>
{
    public void Configure(EntityTypeBuilder<Pricing> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.AmountRial)
            .IsRequired();

        builder.Property(p => p.EffectiveFrom)
            .IsRequired(false);

        builder.Property(p => p.EffectiveTo)
            .IsRequired(false);

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired(false);

        // Seed Data: تعرفه‌های اولیه
        // توجه: مبالغ به ریال هستند (مثلاً 10000 ریال = 1000 تومان)
        builder.HasData(
            new Pricing
            {
                Id = 1,
                Code = "UnlockContactFee",
                Title = "هزینه باز کردن اطلاعات تماس",
                AmountRial = 10000, // 1000 تومان
                IsActive = true,
                EffectiveFrom = DateTime.Now,
                Description = "هزینه یک‌باره برای نمایش اطلاعات تماس یک آگهی",
                CreatedAt = DateTime.Now
            },
            new Pricing
            {
                Id = 2,
                Code = "OnboardingFee",
                Title = "هزینه ثبت‌نام تأمین‌کننده",
                AmountRial = 50000, // 5000 تومان
                IsActive = true,
                EffectiveFrom = DateTime.Now,
                Description = "هزینه یک‌باره ثبت‌نام به عنوان تأمین‌کننده",
                CreatedAt = DateTime.Now
            }
        );
    }
}













