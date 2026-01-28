using IngApp.Domain.Entities.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    // ثابت‌ها برای Seed (IDهای پایدار)
    public static readonly Guid AdminRoleId = Guid.Parse("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1");
    public static readonly Guid BuyerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid SupplierRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public void Configure(EntityTypeBuilder<Role> builder)
    {

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder.Property(r => r.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);

        builder.HasData(
            new Role
            {
                Id = AdminRoleId,
                Name = "Admin",
                DisplayName = "ادمین",
                Description = "دسترسی کامل به سیستم",
                IsActive = true
            },
            new Role
            {
                Id = BuyerRoleId,
                Name = "Buyer",
                DisplayName = "خریدار",
                Description = "دسترسی‌های پایه کاربر",
                IsActive = true
            },
            new Role
            {
                Id = SupplierRoleId,
                Name = "Supplier",
                DisplayName = "تأمین‌کننده",
                Description = "دسترسی‌های پنل تأمین‌کننده",
                IsActive = true
            }
        );
    }
}
