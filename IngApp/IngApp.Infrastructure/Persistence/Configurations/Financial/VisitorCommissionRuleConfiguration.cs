using IngApp.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class VisitorCommissionRuleConfiguration : IEntityTypeConfiguration<VisitorCommissionRule>
{
    public void Configure(EntityTypeBuilder<VisitorCommissionRule> builder)
    {

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.VisitorProfileId)
            .IsRequired();

        builder.Property(x => x.CommissionRuleCode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CommissionPercentage)
            .HasPrecision(5, 2); // مثلاً 99.99%

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Foreign Key
        builder.HasOne(x => x.VisitorProfile)
            .WithMany()
            .HasForeignKey(x => x.VisitorProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index برای جستجوی سریع (بدون Unique - برای تاریخچه)
        builder.HasIndex(x => new { x.VisitorProfileId, x.CommissionRuleCode, x.IsActive })
            .HasDatabaseName("IX_VisitorCommissionRule_Visitor_Code_Active");
        
        // Index برای جستجوی بر اساس تاریخ (برای جلوگیری از هم‌پوشانی)
        builder.HasIndex(x => new { x.VisitorProfileId, x.CommissionRuleCode, x.EffectiveFrom, x.EffectiveTo })
            .HasDatabaseName("IX_VisitorCommissionRule_Visitor_Code_Dates");
    }
}










