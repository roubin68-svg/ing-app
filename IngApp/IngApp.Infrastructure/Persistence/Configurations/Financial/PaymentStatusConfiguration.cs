using IngApp.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class PaymentStatusConfiguration : IEntityTypeConfiguration<PaymentStatus>
{
    public void Configure(EntityTypeBuilder<PaymentStatus> builder)
    {
        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(ps => ps.Code)
            .IsUnique();

        builder.Property(ps => ps.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ps => ps.Description)
            .HasMaxLength(500);

        builder.Property(ps => ps.IsActive)
            .IsRequired();

        // Seed Data
        builder.HasData(
            new PaymentStatus { Id = 1, Code = "Pending", Title = "در انتظار", Description = "پرداخت در انتظار است", IsActive = true },
            new PaymentStatus { Id = 2, Code = "Success", Title = "موفق", Description = "پرداخت با موفقیت انجام شد", IsActive = true },
            new PaymentStatus { Id = 3, Code = "Failed", Title = "ناموفق", Description = "پرداخت ناموفق بود", IsActive = true },
            new PaymentStatus { Id = 4, Code = "Cancelled", Title = "لغو شده", Description = "پرداخت لغو شد", IsActive = true }
        );
    }
}










