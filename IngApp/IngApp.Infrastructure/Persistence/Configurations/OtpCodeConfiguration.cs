using IngApp.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations;

public class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.CodeHash)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Purpose)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        builder.Property(x => x.IsUsed)
            .IsRequired();

        builder.Property(x => x.AttemptCount)
            .IsRequired();

        builder.Property(x => x.ClientIdentifier)
            .HasMaxLength(200);
    }
}
