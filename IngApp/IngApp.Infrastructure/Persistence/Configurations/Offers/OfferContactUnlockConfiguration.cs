using IngApp.Domain.Entities.Offers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Offers;

public class OfferContactUnlockConfiguration : IEntityTypeConfiguration<OfferContactUnlock>
{
    public void Configure(EntityTypeBuilder<OfferContactUnlock> builder)
    {

        builder.HasKey(ocu => ocu.Id);

        builder.Property(ocu => ocu.OfferId)
            .IsRequired();

        builder.HasOne(ocu => ocu.Offer)
            .WithMany()
            .HasForeignKey(ocu => ocu.OfferId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(ocu => ocu.UserId)
            .IsRequired();

        builder.Property(ocu => ocu.UnlockedAt)
            .IsRequired();

        builder.Property(ocu => ocu.ChargedTransactionId)
            .IsRequired(false);

        builder.Property(ocu => ocu.SourceTypeId)
            .IsRequired();

        builder.HasOne(ocu => ocu.SourceType)
            .WithMany()
            .HasForeignKey(ocu => ocu.SourceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique Index: هر کاربر برای هر آگهی فقط یک‌بار Unlock می‌کند
        builder.HasIndex(ocu => new { ocu.OfferId, ocu.UserId })
            .IsUnique();

        // Index برای جستجوی سریع Unlock های یک کاربر
        builder.HasIndex(ocu => ocu.UserId);

        // Index برای جستجوی سریع Unlock های یک آگهی
        builder.HasIndex(ocu => ocu.OfferId);
    }
}












