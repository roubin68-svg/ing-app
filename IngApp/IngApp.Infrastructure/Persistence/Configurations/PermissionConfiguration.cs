using IngApp.Domain.Entities.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    // Permission IDs ثابت (پایدار)
    public static readonly Guid SettingsViewId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    public static readonly Guid UserManageId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002");
    public static readonly Guid RoleManageId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003");
    public static readonly Guid PermissionManageId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000004");
    public static readonly Guid MenuManageId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000005");

    public static readonly Guid ProductViewAllId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000006");
    public static readonly Guid ProductCategoryManageId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000007");

    public static readonly Guid SupplierTypeManageId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000008");
    public static readonly Guid SupplierManageId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000009");
    public static readonly Guid KycReviewId = Guid.Parse("aaaaaaaa-0000-0000-0000-00000000000a");

    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(p => p.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.IsActive)
            .IsRequired();

        // Seed پایه (هم‌راستا با MenuItemConfiguration + نیازهای Supplier/KYC)
        builder.HasData(
            new Permission { Id = SettingsViewId, Code = "Settings.View", DisplayName = "مشاهده تنظیمات", IsActive = true },
            new Permission { Id = UserManageId, Code = "User.Manage", DisplayName = "مدیریت کاربران", IsActive = true },
            new Permission { Id = RoleManageId, Code = "Role.Manage", DisplayName = "مدیریت نقش‌ها", IsActive = true },
            new Permission { Id = PermissionManageId, Code = "Permission.Manage", DisplayName = "مدیریت دسترسی‌ها", IsActive = true },
            new Permission { Id = MenuManageId, Code = "Menu.Manage", DisplayName = "مدیریت منوها", IsActive = true },

            new Permission { Id = ProductViewAllId, Code = "Product.ViewAll", DisplayName = "مشاهده محصولات", IsActive = true },
            new Permission { Id = ProductCategoryManageId, Code = "ProductCategory.Manage", DisplayName = "مدیریت دسته‌بندی محصولات", IsActive = true },

            new Permission { Id = SupplierTypeManageId, Code = "SupplierType.Manage", DisplayName = "مدیریت نوع تأمین‌کننده", IsActive = true },
            new Permission { Id = SupplierManageId, Code = "Supplier.Manage", DisplayName = "مدیریت تأمین‌کنندگان", IsActive = true },
            new Permission { Id = KycReviewId, Code = "Kyc.Review", DisplayName = "بررسی مدارک KYC", IsActive = true }
        );
    }
}
