using IngApp.Domain.Entities.Suppliers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations
{
    public class SupplierVerificationHistoryConfiguration : IEntityTypeConfiguration<SupplierVerificationHistory>
    {
        public void Configure(EntityTypeBuilder<SupplierVerificationHistory> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.SupplierProfile)
                   .WithMany()
                   .HasForeignKey(x => x.SupplierProfileId);

            builder.Property(x => x.OldStatus).IsRequired();
            builder.Property(x => x.NewStatus).IsRequired();

            builder.Property(x => x.CreatedAt).IsRequired();
        }
    }
}
