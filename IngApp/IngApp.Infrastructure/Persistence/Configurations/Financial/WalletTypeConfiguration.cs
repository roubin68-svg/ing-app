using IngApp.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class WalletTypeConfiguration : IEntityTypeConfiguration<WalletType>
{
    public void Configure(EntityTypeBuilder<WalletType> builder)
    {
        builder.HasKey(wt => wt.Id);

        builder.Property(wt => wt.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(wt => wt.Code)
            .IsUnique();

        builder.Property(wt => wt.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wt => wt.Description)
            .HasMaxLength(500);

        builder.Property(wt => wt.IsActive)
            .IsRequired();

        // Seed Data: WalletType های اولیه
        builder.HasData(
            new WalletType 
            { 
                Id = 1, 
                Code = "Main", 
                Title = "کیف پول اصلی", 
                IsActive = true 
            }
        );
    }
}











