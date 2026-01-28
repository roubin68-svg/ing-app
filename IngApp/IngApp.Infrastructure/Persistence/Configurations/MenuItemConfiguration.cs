using IngApp.Domain.Entities.Menus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(m => m.Key)
            .IsUnique();

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Route)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Icon)
            .HasMaxLength(100);

        builder.Property(m => m.RequiredPermissionCode)
            .HasMaxLength(150);

        builder.HasOne(m => m.Parent)
            .WithMany(m => m.Children)
            .HasForeignKey(m => m.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(

             // Dashboard
             new MenuItem { Id = 1, Key = "dashboard", Title = "داشبورد", Route = "/", Icon = "DashboardOutlined", Order = 1, ParentId = null, RequiredPermissionCode = null, IsActive = true },

             // Products
             new MenuItem { Id = 2, Key = "products", Title = "مدیریت محصولات", Route = "#", Icon = "ShoppingOutlined", Order = 2, ParentId = null, RequiredPermissionCode = "Product.ViewAll", IsActive = true },

             // Products/Product List
             new MenuItem { Id = 3, Key = "products-list", Title = "لیست محصولات", Route = "/products", Order = 1, ParentId = 2, RequiredPermissionCode = "Product.ViewAll", IsActive = true },

             // Products/Category Management
             new MenuItem { Id = 4, Key = "products-categories", Title = "دسته‌بندی محصولات", Route = "/product-categories", Order = 2, ParentId = 2, RequiredPermissionCode = "ProductCategory.Manage", IsActive = true },

             // Offer Management
             new MenuItem { Id = 18, Key = "offer-managment", Title = "مدیریت آگهی ها", Route = "/offer-managment", Icon = "FileTextOutlined", Order = 2, ParentId = null, RequiredPermissionCode = "Offer.Manage", IsActive = true },

             // User Management
             new MenuItem { Id = 6, Key = "user-management", Title = "مدیریت کاربران", Route = "#", Icon = "TeamOutlined", Order = 5, ParentId = null, RequiredPermissionCode = "User.Manage", IsActive = true },

             // User Management/Users
             new MenuItem { Id = 7, Key = "users", Title = "کاربران", Route = "/users", Order = 1, ParentId = 6, RequiredPermissionCode = "User.Manage", IsActive = true },

             // User Management/Roles
             new MenuItem { Id = 8, Key = "roles", Title = "نقش‌ها", Route = "/roles", Order = 2, ParentId = 6, RequiredPermissionCode = "Role.Manage", IsActive = true },

             // User Management/Permissions
             new MenuItem { Id = 9, Key = "permissions", Title = "دسترسی‌ها", Route = "/permissions", Order = 3, ParentId = 6, RequiredPermissionCode = "Permission.Manage", IsActive = true },

             // Financial
             new MenuItem { Id = 22, Key = "financial", Title = "سیستم مالی", Route = "#", Icon = "WalletOutlined", Order = 6, ParentId = null, RequiredPermissionCode = null, IsActive = true },

             // Financial/Subscriptions
             new MenuItem { Id = 1001, Key = "subscriptions", Title = "اشتراک‌ها", Route = "/subscriptions", Order = 1, ParentId = 22, RequiredPermissionCode = null, IsActive = true },

             // Financial/Top Up
             new MenuItem { Id = 1002, Key = "top-up", Title = "شارژ کیف پول", Route = "/top-up", Order = 2, ParentId = 22, RequiredPermissionCode = null, IsActive = true },

             // Financial/Wallet Transactions
             new MenuItem { Id = 1003, Key = "wallet-transactions", Title = "تراکنش‌های کیف پول", Route = "/wallet-transactions", Order = 3, ParentId = 22, RequiredPermissionCode = null, IsActive = true },

             // Financial/Commission Rules
             new MenuItem { Id = 1006, Key = "commission-rules", Title = "قوانین پورسانت", Route = "/commission-rules", Order = 4, ParentId = 22, RequiredPermissionCode = null, IsActive = true },

             // Supplier Types (Standalone)
             new MenuItem { Id = 12, Key = "supplier-types", Title = "نوع تامین کننده", Route = "/supplier-types", Order = 3, ParentId = null, RequiredPermissionCode = null, IsActive = true },

             // KYC Templates (Standalone)
             new MenuItem { Id = 15, Key = "kyc-templates", Title = "قالب‌های KYC", Route = "/kyc-templates", Order = 4, ParentId = null, RequiredPermissionCode = null, IsActive = true },

             // Visitor Management
             new MenuItem { Id = 1007, Key = "visitor-management", Title = "مدیریت بازاریابان", Route = "/visitor-management", Order = 7, ParentId = null, RequiredPermissionCode = null, IsActive = true },

             // Buyer Profiles
             new MenuItem { Id = 1008, Key = "buyer-profiles", Title = "پروفایل خریداران", Route = "/buyer-profiles", Order = 8, ParentId = null, RequiredPermissionCode = null, IsActive = true }

             );

    }
}
