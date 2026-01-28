using IngApp.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class FinancialOperationTypeConfiguration : IEntityTypeConfiguration<FinancialOperationType>
{
    public void Configure(EntityTypeBuilder<FinancialOperationType> builder)
    {
        builder.HasKey(fot => fot.Id);

        builder.Property(fot => fot.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(fot => fot.Code)
            .IsUnique();

        builder.Property(fot => fot.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(fot => fot.Description)
            .HasMaxLength(500);

        builder.Property(fot => fot.IsActive)
            .IsRequired();

        // Seed Data: FinancialOperationType های اولیه
        builder.HasData(
            new FinancialOperationType { Id = 1, Code = "TopUp", Title = "شارژ کیف پول", Description = "واریز وجه به کیف پول", IsActive = true },
            new FinancialOperationType { Id = 2, Code = "UnlockContactFee", Title = "هزینه باز کردن اطلاعات تماس", Description = "هزینه نمایش اطلاعات تماس آگهی", IsActive = true },
            new FinancialOperationType { Id = 3, Code = "SubscriptionPurchase", Title = "خرید اشتراک", Description = "خرید پکیج/اشتراک", IsActive = true },
            new FinancialOperationType { Id = 4, Code = "OnboardingFee", Title = "هزینه ثبت‌نام تأمین‌کننده", Description = "هزینه یک‌باره ثبت‌نام به عنوان تأمین‌کننده", IsActive = true },
            new FinancialOperationType { Id = 5, Code = "CommissionEarned", Title = "دریافت پورسانت", Description = "پورسانت دریافتی از بازاریابی", IsActive = true },
            new FinancialOperationType { Id = 6, Code = "ManualDeposit", Title = "واریز دستی", Description = "واریز دستی توسط مدیر", IsActive = true },
            new FinancialOperationType { Id = 7, Code = "ManualWithdrawal", Title = "برداشت دستی", Description = "برداشت دستی توسط مدیر", IsActive = true }
        );
    }
}











