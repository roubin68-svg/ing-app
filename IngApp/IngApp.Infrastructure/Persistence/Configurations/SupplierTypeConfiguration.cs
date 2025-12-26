using IngApp.Domain.Entities.Suppliers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations
{
    public class SupplierTypeConfiguration : IEntityTypeConfiguration<SupplierType>
    {
        public void Configure(EntityTypeBuilder<SupplierType> builder)
        {
            builder.ToTable("SupplierTypes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.IsActive)
                .IsRequired();

        }
    }
}
