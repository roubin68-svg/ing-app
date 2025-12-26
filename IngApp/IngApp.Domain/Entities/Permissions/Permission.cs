using IngApp.Domain.Entities.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngApp.Domain.Entities.Permissions;

public class Permission
{
    public Guid Id { get; set; }

    // کد سیستمی که در Claim و چک کردن استفاده می‌کنیم
    public string Code { get; set; } = string.Empty;       // مثال: "Product.ViewAll"
    public string DisplayName { get; set; } = string.Empty; // برای UI
    public string? Description { get; set; } = string.Empty; 

    public bool IsActive { get; set; } = true;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();





}
