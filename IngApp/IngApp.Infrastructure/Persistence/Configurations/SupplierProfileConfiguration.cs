using IngApp.Domain.Entities.Suppliers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations
{
    public class SupplierProfileConfiguration : IEntityTypeConfiguration<SupplierProfile>
    {
        public void Configure(EntityTypeBuilder<SupplierProfile> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.BusinessName).HasMaxLength(300).IsRequired();

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId);

            builder.HasOne(x => x.SupplierType)
                   .WithMany()
                   .HasForeignKey(x => x.SupplierTypeId);

            builder.Property(x => x.CreatedAt).IsRequired();
        }
    }
}
