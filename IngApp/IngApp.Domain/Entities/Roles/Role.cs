using System.Collections.Generic;

namespace IngApp.Domain.Entities.Roles;

public class Role
{
    public Guid Id { get; set; }

    /// <summary>
    /// نام انگلیسی نقش (برای سیستم و کد)
    /// مثال: Admin, ProductManager
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// نام نمایشی فارسی برای UI
    /// مثال: مدیر کل، مدیر محصولات
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();





}
