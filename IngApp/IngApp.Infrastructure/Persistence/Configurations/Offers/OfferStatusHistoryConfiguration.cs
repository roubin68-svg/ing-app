using IngApp.Domain.Entities.Offers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Offers;

public class OfferStatusHistoryConfiguration : IEntityTypeConfiguration<OfferStatusHistory>
{
    public void Configure(EntityTypeBuilder<OfferStatusHistory> builder)
    {

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Offer)
               .WithMany()
               .HasForeignKey(x => x.OfferId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.OldStatus).IsRequired();
        builder.Property(x => x.NewStatus).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.Note).HasMaxLength(1000);

        builder.HasIndex(x => x.OfferId);
        builder.HasIndex(x => x.CreatedAt);
    }
}












