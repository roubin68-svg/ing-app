using IngApp.Domain.Entities.Kyc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations
{
    public class KycAttributeDefinitionConfiguration : IEntityTypeConfiguration<KycAttributeDefinition>
    {
        public void Configure(EntityTypeBuilder<KycAttributeDefinition> builder)
        {

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DisplayName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();
        }
    }
}
