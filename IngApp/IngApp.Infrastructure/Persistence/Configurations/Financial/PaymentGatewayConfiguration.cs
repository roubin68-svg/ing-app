using IngApp.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class PaymentGatewayConfiguration : IEntityTypeConfiguration<PaymentGateway>
{
    public void Configure(EntityTypeBuilder<PaymentGateway> builder)
    {
        builder.HasKey(pg => pg.Id);

        builder.Property(pg => pg.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(pg => pg.Code)
            .IsUnique();

        builder.Property(pg => pg.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pg => pg.Description)
            .HasMaxLength(500);

        builder.Property(pg => pg.IsActive)
            .IsRequired();

        // Seed Data
        builder.HasData(
            new PaymentGateway { Id = 1, Code = "Mock", Title = "درگاه پرداخت آزمایشی", Description = "درگاه پرداخت Mock برای تست", IsActive = true },
            new PaymentGateway { Id = 2, Code = "Zarinpal", Title = "زرین‌پال", Description = "درگاه پرداخت زرین‌پال", IsActive = false } // برای آینده
        );
    }
}











