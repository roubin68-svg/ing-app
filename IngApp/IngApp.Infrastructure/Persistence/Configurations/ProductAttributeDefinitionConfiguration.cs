using IngApp.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations;

public class ProductAttributeDefinitionConfiguration : IEntityTypeConfiguration<ProductAttributeDefinition>
{
    public void Configure(EntityTypeBuilder<ProductAttributeDefinition> builder)
    {
        builder.ToTable("ProductAttributeDefinitions");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.DataType)
            .IsRequired();

        builder.Property(a => a.Unit)
            .HasMaxLength(50);

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true);
    }
}
