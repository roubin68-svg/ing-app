using IngApp.Domain.Entities.Kyc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations
{
    public class UserDocumentConfiguration : IEntityTypeConfiguration<UserDocument>
    {
        public void Configure(EntityTypeBuilder<UserDocument> builder)
        {
            builder.ToTable("UserDocuments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FilePath)
                .HasMaxLength(500);

            builder.Property(x => x.Value)
                .HasMaxLength(500);

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.HasIndex(x => new { x.UserId, x.IsDeleted });
            builder.HasIndex(x => new { x.UserId, x.KycAttributeDefinitionId, x.IsDeleted });
        }
    }
}
