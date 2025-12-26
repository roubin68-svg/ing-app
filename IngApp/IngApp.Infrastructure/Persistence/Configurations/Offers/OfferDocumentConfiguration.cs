using IngApp.Domain.Entities.Offers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Offers;

public class OfferDocumentConfiguration : IEntityTypeConfiguration<OfferDocument>
{
    public void Configure(EntityTypeBuilder<OfferDocument> builder)
    {
        builder.ToTable("OfferDocuments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AttributeDefinitionId)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(1000);

        builder.Property(x => x.FilePath)
            .HasMaxLength(1000);
        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.DeletedAt);

        builder.HasIndex(x => new { x.OfferId, x.IsDeleted });


        builder.HasIndex(x => x.OfferId);
        builder.HasIndex(x => x.AttributeDefinitionId);
    }
}
