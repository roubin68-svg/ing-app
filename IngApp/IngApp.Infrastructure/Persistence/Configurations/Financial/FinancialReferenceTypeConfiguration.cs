using IngApp.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class FinancialReferenceTypeConfiguration : IEntityTypeConfiguration<FinancialReferenceType>
{
    public void Configure(EntityTypeBuilder<FinancialReferenceType> builder)
    {
        builder.HasKey(frt => frt.Id);

        builder.Property(frt => frt.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(frt => frt.Code)
            .IsUnique();

        builder.Property(frt => frt.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(frt => frt.Description)
            .HasMaxLength(500);

        builder.Property(frt => frt.IsActive)
            .IsRequired();

        // Seed Data: FinancialReferenceType های اولیه
        builder.HasData(
            new FinancialReferenceType { Id = 1, Code = "Offer", Title = "آگهی", Description = "مرجع: آگهی", IsActive = true },
            new FinancialReferenceType { Id = 2, Code = "Subscription", Title = "اشتراک", Description = "مرجع: اشتراک/پکیج", IsActive = true },
            new FinancialReferenceType { Id = 3, Code = "Payment", Title = "پرداخت", Description = "مرجع: پرداخت/شارژ", IsActive = true },
            new FinancialReferenceType { Id = 4, Code = "SupplierOnboarding", Title = "ثبت‌نام تأمین‌کننده", Description = "مرجع: ثبت‌نام تأمین‌کننده", IsActive = true },
            new FinancialReferenceType { Id = 5, Code = "WalletTransaction", Title = "تراکنش کیف پول", Description = "مرجع: تراکنش دیگر (مثلاً برای پورسانت)", IsActive = true },
            new FinancialReferenceType { Id = 6, Code = "AdminAction", Title = "عملیات مدیر", Description = "مرجع: عملیات دستی توسط مدیر", IsActive = true }
        );
    }
}











