using IngApp.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class FinancialTransactionStatusConfiguration : IEntityTypeConfiguration<FinancialTransactionStatus>
{
    public void Configure(EntityTypeBuilder<FinancialTransactionStatus> builder)
    {
        builder.HasKey(fts => fts.Id);

        builder.Property(fts => fts.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(fts => fts.Code)
            .IsUnique();

        builder.Property(fts => fts.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(fts => fts.Description)
            .HasMaxLength(500);

        builder.Property(fts => fts.IsActive)
            .IsRequired();

        // Seed Data: FinancialTransactionStatus های اولیه
        builder.HasData(
            new FinancialTransactionStatus { Id = 1, Code = "Pending", Title = "در انتظار", Description = "تراکنش در حال پردازش", IsActive = true },
            new FinancialTransactionStatus { Id = 2, Code = "Committed", Title = "تأیید شده", Description = "تراکنش با موفقیت انجام شد", IsActive = true },
            new FinancialTransactionStatus { Id = 3, Code = "Failed", Title = "ناموفق", Description = "تراکنش با خطا مواجه شد", IsActive = true },
            new FinancialTransactionStatus { Id = 4, Code = "Reversed", Title = "برگشت خورده", Description = "تراکنش برگشت داده شد", IsActive = true }
        );
    }
}











