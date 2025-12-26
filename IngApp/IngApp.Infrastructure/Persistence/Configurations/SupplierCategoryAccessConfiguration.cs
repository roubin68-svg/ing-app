using IngApp.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations;

public class SupplierCategoryAccessConfiguration : IEntityTypeConfiguration<SupplierCategoryAccess>
{
    public void Configure(EntityTypeBuilder<SupplierCategoryAccess> builder)
    {
        builder.ToTable("SupplierCategoryAccesses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ProductCategoryId)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.ProductCategory)
            .WithMany()
            .HasForeignKey(x => x.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.ProductCategoryId })
            .IsUnique();
    }
}
