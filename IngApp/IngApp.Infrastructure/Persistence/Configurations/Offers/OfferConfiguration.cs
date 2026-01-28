using IngApp.Domain.Entities.Offers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Offers;

public class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {

        builder.HasKey(x => x.Id);

        // --------------------
        // Identity
        // --------------------
        builder.Property(x => x.SupplierUserId)
            .IsRequired();

        builder.Property(x => x.ProductId)
            .IsRequired();

        // --------------------
        // Commercial
        // --------------------
        builder.Property(x => x.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.TotalPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.HasTax)
            .IsRequired();

        builder.Property(x => x.TaxAmount)
            .HasPrecision(18, 2);

        // --------------------
        // Lifecycle
        // --------------------
        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.SearchDateTime)
            .IsRequired();

        // --------------------
        // Reason fields
        // --------------------
        builder.Property(x => x.CancelReason)
            .HasMaxLength(500);

        builder.Property(x => x.RejectedReason)
            .HasMaxLength(500);

        builder.Property(x => x.WizardStep)
            .IsRequired();


        // --------------------
        // Relations
        // --------------------
        builder.HasMany(x => x.Documents)
            .WithOne(x => x.Offer)
            .HasForeignKey(x => x.OfferId)
            .OnDelete(DeleteBehavior.Cascade);

        // --------------------
        // Indexes (Search / Supplier)
        // --------------------
        builder.HasIndex(x => x.SupplierUserId);
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.SearchDateTime);
    }
}
