using IngApp.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class UnlockSourceTypeConfiguration : IEntityTypeConfiguration<UnlockSourceType>
{
    public void Configure(EntityTypeBuilder<UnlockSourceType> builder)
    {
        builder.HasKey(ust => ust.Id);

        builder.Property(ust => ust.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(ust => ust.Code)
            .IsUnique();

        builder.Property(ust => ust.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ust => ust.Description)
            .HasMaxLength(500);

        builder.Property(ust => ust.IsActive)
            .IsRequired();

        // Seed Data: UnlockSourceType های اولیه
        builder.HasData(
            new UnlockSourceType 
            { 
                Id = 1, 
                Code = "Paid", 
                Title = "پرداخت شده", 
                Description = "از طریق پرداخت از کیف پول",
                IsActive = true 
            },
            new UnlockSourceType 
            { 
                Id = 2, 
                Code = "Subscription", 
                Title = "اشتراک", 
                Description = "از طریق اشتراک فعال",
                IsActive = true 
            }
        );
    }
}












