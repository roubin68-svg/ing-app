using IngApp.Domain.Entities.Financial;
using IngApp.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class CommissionTransactionConfiguration : IEntityTypeConfiguration<CommissionTransaction>
{
    public void Configure(EntityTypeBuilder<CommissionTransaction> builder)
    {
        builder.HasKey(ct => ct.Id);

        builder.Property(ct => ct.VisitorUserId)
            .IsRequired();

        builder.Property(ct => ct.BuyerUserId)
            .IsRequired();

        builder.Property(ct => ct.CommissionType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ct => ct.OriginalAmountRial)
            .IsRequired();

        builder.Property(ct => ct.CommissionAmountRial)
            .IsRequired();

        builder.Property(ct => ct.CommissionPercentage)
            .HasPrecision(5, 2)
            .IsRequired();

        // Relationships برای CommissionRule و VisitorCommissionRule
        builder.HasOne(ct => ct.CommissionRule)
            .WithMany()
            .HasForeignKey(ct => ct.CommissionRuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ct => ct.VisitorCommissionRule)
            .WithMany()
            .HasForeignKey(ct => ct.VisitorCommissionRuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(ct => ct.ReferenceType)
            .HasMaxLength(50);

        builder.Property(ct => ct.Description)
            .HasMaxLength(1000);

        builder.Property(ct => ct.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(ct => ct.VisitorUser)
            .WithMany()
            .HasForeignKey(ct => ct.VisitorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ct => ct.BuyerUser)
            .WithMany()
            .HasForeignKey(ct => ct.BuyerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ct => ct.VisitorUserId);
        builder.HasIndex(ct => ct.BuyerUserId);
        builder.HasIndex(ct => new { ct.VisitorUserId, ct.CommissionType });
        builder.HasIndex(ct => ct.ReferenceId);
    }
}










