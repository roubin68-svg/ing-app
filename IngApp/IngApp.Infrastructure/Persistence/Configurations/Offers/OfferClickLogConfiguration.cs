using IngApp.Domain.Entities.Offers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Offers;

public class OfferClickLogConfiguration : IEntityTypeConfiguration<OfferClickLog>
{
    public void Configure(EntityTypeBuilder<OfferClickLog> builder)
    {

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OfferId)
            .IsRequired();

        builder.Property(x => x.ClickType)
            .IsRequired();

        builder.Property(x => x.ClickedAt)
            .IsRequired();

        builder.Property(x => x.IpAddress)
            .HasMaxLength(50);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        // Relations
        builder.HasOne(x => x.Offer)
            .WithMany()
            .HasForeignKey(x => x.OfferId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index برای بهبود performance در query‌های آمار
        builder.HasIndex(x => new { x.OfferId, x.ClickType });
        builder.HasIndex(x => x.ClickedAt);
    }
}

