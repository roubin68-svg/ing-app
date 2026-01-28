using IngApp.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations;

public class ProductAttributeTemplateConfiguration : IEntityTypeConfiguration<ProductAttributeTemplate>
{
    public void Configure(EntityTypeBuilder<ProductAttributeTemplate> builder)
    {

        builder.HasKey(t => t.Id);

        builder.Property(t => t.IsRequired)
            .IsRequired();

        builder.HasOne(t => t.Product)
            .WithMany(p => p.AttributeTemplates)
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.AttributeDefinition)
            .WithMany()
            .HasForeignKey(t => t.AttributeDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // 🔒 Unique Constraint (قفل‌شده)
        builder.HasIndex(t => new { t.ProductId, t.AttributeDefinitionId })
            .IsUnique();
    }
}
