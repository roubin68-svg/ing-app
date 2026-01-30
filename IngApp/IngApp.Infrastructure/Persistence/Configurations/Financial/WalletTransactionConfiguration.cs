using IngApp.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.HasKey(wt => wt.Id);

        builder.Property(wt => wt.WalletId)
            .IsRequired();

        builder.HasOne(wt => wt.Wallet)
            .WithMany(w => w.Transactions)
            .HasForeignKey(wt => wt.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(wt => wt.DirectionId)
            .IsRequired();

        builder.HasOne(wt => wt.Direction)
            .WithMany()
            .HasForeignKey(wt => wt.DirectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(wt => wt.AmountRial)
            .IsRequired();

        builder.Property(wt => wt.OperationTypeId)
            .IsRequired();

        builder.HasOne(wt => wt.OperationType)
            .WithMany()
            .HasForeignKey(wt => wt.OperationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(wt => wt.StatusId)
            .IsRequired();

        builder.HasOne(wt => wt.Status)
            .WithMany()
            .HasForeignKey(wt => wt.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(wt => wt.ReferenceTypeId)
            .IsRequired();

        builder.HasOne(wt => wt.ReferenceType)
            .WithMany()
            .HasForeignKey(wt => wt.ReferenceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(wt => wt.ReferenceId)
            .IsRequired(false);

        builder.Property(wt => wt.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(100);

        // IdempotencyKey باید یکتا باشد
        builder.HasIndex(wt => wt.IdempotencyKey)
            .IsUnique();

        builder.Property(wt => wt.Description)
            .HasMaxLength(500);

        builder.Property(wt => wt.IsBankSettlement)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(wt => wt.CreatedAt)
            .IsRequired();

        // Index برای جستجوی سریع تراکنش‌های یک Wallet
        builder.HasIndex(wt => wt.WalletId);

        // Index برای جستجوی تراکنش‌ها بر اساس Reference
        builder.HasIndex(wt => new { wt.ReferenceTypeId, wt.ReferenceId });
    }
}











