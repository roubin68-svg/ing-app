using IngApp.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.UserId)
            .IsRequired();

        // هر کاربر فقط یک Wallet با WalletType=Main می‌تواند داشته باشد
        builder.HasIndex(w => new { w.UserId, w.WalletTypeId })
            .IsUnique();

        builder.Property(w => w.CurrencyId)
            .IsRequired();

        builder.HasOne(w => w.Currency)
            .WithMany()
            .HasForeignKey(w => w.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(w => w.WalletTypeId)
            .IsRequired();

        builder.HasOne(w => w.WalletType)
            .WithMany()
            .HasForeignKey(w => w.WalletTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(w => w.BalanceRial)
            .IsRequired()
            .HasDefaultValue(0L);

        // Concurrency Token
        builder.Property(w => w.RowVersion)
            .IsRowVersion()
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .IsRequired();

        builder.HasOne(w => w.User)
            .WithOne(u => u.Wallet)
            .HasForeignKey<Wallet>(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}












