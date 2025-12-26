using IngApp.Domain.Entities.Users;
using IngApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public static readonly Guid DefaultAdminUserId = Guid.Parse("64fa4b00-95cf-4a58-6f40-08de38f0e8e0");

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(u => u.PhoneNumber)
            .IsUnique();

        builder.Property(u => u.DisplayName)
            .HasMaxLength(200);

        builder.Property(u => u.UserType)
            .IsRequired();

        builder.Property(u => u.SubscriptionLevel)
            .IsRequired();

        builder.Property(u => u.VerificationStatus)
            .IsRequired();

        builder.Property(u => u.IsActive)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId);

        builder.HasMany(u => u.Documents)
            .WithOne(d => d.User)
            .HasForeignKey(d => d.UserId);

        // Seed: کاربر ادمین پیش‌فرض
        builder.HasData(
            new User
            {
                Id = DefaultAdminUserId,
                PhoneNumber = "09123823632",
                DisplayName = "علی هور",
                UserType = UserType.Admin,
                SubscriptionLevel = SubscriptionLevel.None,
                VerificationStatus = VerificationStatus.NotSubmitted,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
