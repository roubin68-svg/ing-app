using IngApp.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(s => s.Key)
            .IsUnique();

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(s => s.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.DataType)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("String");

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        // Seed: کارمزد خدمات لغو اشتراک (پیش‌فرض 10%)
        builder.HasData(
            new SystemSetting
            {
                Id = 1,
                Key = "SubscriptionCancellationServiceFeePercentage",
                Value = "10",
                DisplayName = "کارمزد خدمات لغو اشتراک (درصد)",
                Description = "درصد کارمزد خدمات که از مبلغ برگشتی اشتراک کسر می‌شود",
                DataType = "Number",
                CreatedAt = DateTime.Now
            }
        );
    }
}



