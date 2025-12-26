namespace IngApp.Application.Features.Roles.DTO;

public class AssignPermissionsToRoleDto
{
    /// <summary>
    /// لیست کد Permissionها که می‌خواهیم به نقش بدهیم
    /// مثال: "User.Manage", "Product.ViewAll"
    /// </summary>
    public List<string> PermissionCodes { get; set; } = new();
}
