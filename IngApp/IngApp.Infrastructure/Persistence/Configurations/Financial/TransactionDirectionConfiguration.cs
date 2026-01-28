using IngApp.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class TransactionDirectionConfiguration : IEntityTypeConfiguration<TransactionDirection>
{
    public void Configure(EntityTypeBuilder<TransactionDirection> builder)
    {
        builder.HasKey(td => td.Id);

        builder.Property(td => td.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(td => td.Code)
            .IsUnique();

        builder.Property(td => td.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(td => td.Description)
            .HasMaxLength(500);

        builder.Property(td => td.IsActive)
            .IsRequired();

        // Seed Data: TransactionDirection های اولیه
        builder.HasData(
            new TransactionDirection 
            { 
                Id = 1, 
                Code = "Credit", 
                Title = "واریز", 
                Description = "افزایش موجودی کیف پول",
                IsActive = true 
            },
            new TransactionDirection 
            { 
                Id = 2, 
                Code = "Debit", 
                Title = "برداشت", 
                Description = "کاهش موجودی کیف پول",
                IsActive = true 
            }
        );
    }
}











