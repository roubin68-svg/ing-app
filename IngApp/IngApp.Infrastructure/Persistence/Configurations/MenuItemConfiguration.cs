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
             new MenuItem { Id = 1, Key = "dashboard", Title = "داشبورد", Route = "/", Icon = "DashboardOutlined", Order = 1, ParentId = null, RequiredPermissionCode = null },

             // Products
             new MenuItem { Id = 2, Key = "products", Title = "مدیریت محصولات", Route = "#", Icon = "ShoppingOutlined", Order = 2, ParentId = null, RequiredPermissionCode = "Product.ViewAll" },

             // Products/Product List
             new MenuItem { Id = 3, Key = "products-list", Title = "لیست محصولات", Route = "/products", Order = 1, ParentId = 2, RequiredPermissionCode = "Product.ViewAll" },

             // Products/Category Management
             new MenuItem { Id = 4, Key = "category-management", Title = "مدیریت دسته‌بندی‌ها", Route = "/product-categories", Order = 2, ParentId = 2, RequiredPermissionCode = "ProductCategory.Manage" },

             // Settings
             new MenuItem { Id = 5, Key = "settings", Title = "تنظیمات", Route = "#", Icon = "SettingOutlined", Order = 4, ParentId = null, RequiredPermissionCode = "Settings.View" },

             // Settings/Menu Settings
             new MenuItem { Id = 10, Key = "menu-settings", Title = "تنظیمات منو", Route = "/menu-settings", Order = 2, ParentId = 5, RequiredPermissionCode = "Menu.Manage" },

             // User Management
             new MenuItem { Id = 6, Key = "user-management", Title = "مدیریت کاربران", Route = "#", Icon = "TeamOutlined", Order = 3, ParentId = null, RequiredPermissionCode = "User.Manage" },

             // User Management/Users
             new MenuItem { Id = 7, Key = "users", Title = "کاربران", Route = "/users", Order = 1, ParentId = 6, RequiredPermissionCode = "User.Manage" },

             // User Management/Roles
             new MenuItem { Id = 8, Key = "roles", Title = "نقش‌ها", Route = "/roles", Order = 2, ParentId = 6, RequiredPermissionCode = "Role.Manage" },

             // User Management/Permissions
             new MenuItem { Id = 9, Key = "permissions", Title = "دسترسی‌ها", Route = "/permissions", Order = 3, ParentId = 6, RequiredPermissionCode = "Permission.Manage" },

             // Suppliers
             new MenuItem { Id = 11, Key = "suppliers", Title = "مدیریت تأمین‌کنندگان", Route = "#", Icon = "TeamOutlined", Order = 5, ParentId = null, RequiredPermissionCode = "Supplier.View" },

             // Suppliers/suppliers list
            // new MenuItem { Id = 14, Key = "suppliers-list", Title = "لیست تامین کنندگان", Route = "/suppliers-list", Order = 1, ParentId = 11, RequiredPermissionCode = "SuppliersList.Manage" },

             // Suppliers/supplier types
             new MenuItem { Id = 12, Key = "supplier-types", Title = "مدیریت نوع تأمین‌کننده", Route = "/supplier-types", Order = 2, ParentId = 11, RequiredPermissionCode = "SupplierType.Manage" }

             // Suppliers/kyc- attribute definitions
             //new MenuItem { Id = 13, Key = "kyc-attribute-definitions", Title = "مدیریت مدارک احراز هویت", Route = "/kyc-attribute-definitions", Order = 3, ParentId = 11, RequiredPermissionCode = "KycAttributeDefinitions.Manage" },

             // Suppliers/kyc templates 
             //new MenuItem { Id = 15, Key = "kyc-templates", Title = "مدیریت الگوی مدارک احراز هویت", Route = "/kyc-templates", Order = 4, ParentId = 11, RequiredPermissionCode = "KycTemplates.Manage" }


             );

    }
}
