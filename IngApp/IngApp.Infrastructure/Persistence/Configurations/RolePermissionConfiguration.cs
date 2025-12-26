using IngApp.Domain.Entities.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        builder.HasOne(rp => rp.Role)
               .WithMany(r => r.RolePermissions)
               .HasForeignKey(rp => rp.RoleId);

        builder.HasOne(rp => rp.Permission)
               .WithMany()
               .HasForeignKey(rp => rp.PermissionId);

        // Seed: Admin => همه Permissionهای پایه
        builder.HasData(
            new RolePermission { RoleId = RoleConfiguration.AdminRoleId, PermissionId = PermissionConfiguration.SettingsViewId },
            new RolePermission { RoleId = RoleConfiguration.AdminRoleId, PermissionId = PermissionConfiguration.UserManageId },
            new RolePermission { RoleId = RoleConfiguration.AdminRoleId, PermissionId = PermissionConfiguration.RoleManageId },
            new RolePermission { RoleId = RoleConfiguration.AdminRoleId, PermissionId = PermissionConfiguration.PermissionManageId },
            new RolePermission { RoleId = RoleConfiguration.AdminRoleId, PermissionId = PermissionConfiguration.MenuManageId },
            new RolePermission { RoleId = RoleConfiguration.AdminRoleId, PermissionId = PermissionConfiguration.ProductViewAllId },
            new RolePermission { RoleId = RoleConfiguration.AdminRoleId, PermissionId = PermissionConfiguration.ProductCategoryManageId },
            new RolePermission { RoleId = RoleConfiguration.AdminRoleId, PermissionId = PermissionConfiguration.SupplierTypeManageId },
            new RolePermission { RoleId = RoleConfiguration.AdminRoleId, PermissionId = PermissionConfiguration.SupplierManageId },
            new RolePermission { RoleId = RoleConfiguration.AdminRoleId, PermissionId = PermissionConfiguration.KycReviewId }
        );
    }
}
