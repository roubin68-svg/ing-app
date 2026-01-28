using IngApp.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Symbol)
            .HasMaxLength(10);

        builder.Property(c => c.ExchangeRateToRial)
            .IsRequired()
            .HasPrecision(18, 6);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.IsActive)
            .IsRequired();

        // Seed Data: Currency های اولیه
        builder.HasData(
            new Currency 
            { 
                Id = 1, 
                Code = "IRR", 
                Title = "ریال ایران", 
                Symbol = "ریال",
                ExchangeRateToRial = 1,
                IsActive = true 
            }
        );
    }
}












