using IngApp.Domain.Entities.Financial;
using IngApp.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.GatewayId)
            .IsRequired();

        builder.Property(p => p.StatusId)
            .IsRequired();

        builder.Property(p => p.AmountRial)
            .IsRequired();

        builder.Property(p => p.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(p => p.IdempotencyKey)
            .IsUnique();

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.GatewayResponseJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Gateway)
            .WithMany()
            .HasForeignKey(p => p.GatewayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Status)
            .WithMany()
            .HasForeignKey(p => p.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => new { p.UserId, p.StatusId });
        builder.HasIndex(p => p.GatewayTransactionId);
    }
}











