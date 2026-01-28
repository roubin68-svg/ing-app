using IngApp.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations;

public class UserTypeConfiguration : IEntityTypeConfiguration<UserType>
{
    public void Configure(EntityTypeBuilder<UserType> builder)
    {
        builder.HasKey(ut => ut.Id);

        builder.Property(ut => ut.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(ut => ut.Code)
            .IsUnique();

        builder.Property(ut => ut.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ut => ut.Description)
            .HasMaxLength(500);

        builder.Property(ut => ut.IsActive)
            .IsRequired();

        // Seed Data: UserType های اولیه
        builder.HasData(
            new UserType { Id = 1, Code = "Buyer", Title = "خریدار", IsActive = true },
            new UserType { Id = 2, Code = "Supplier", Title = "تأمین‌کننده", IsActive = true },
            new UserType { Id = 3, Code = "Admin", Title = "مدیر سیستم", IsActive = true },
            new UserType { Id = 4, Code = "Visitor", Title = "بازاریاب", IsActive = true }
        );
    }
}












