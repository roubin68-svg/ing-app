using IngApp.Domain.Entities.Suppliers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations
{
    public class SupplierActivityLogConfiguration : IEntityTypeConfiguration<SupplierActivityLog>
    {
        public void Configure(EntityTypeBuilder<SupplierActivityLog> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ActionType)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.HasOne(x => x.SupplierProfile)
                   .WithMany()
                   .HasForeignKey(x => x.SupplierProfileId);

            builder.Property(x => x.CreatedAt).IsRequired();
        }
    }
}
