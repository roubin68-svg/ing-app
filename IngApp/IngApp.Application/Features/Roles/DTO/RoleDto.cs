namespace IngApp.Application.Features.Roles.DTO;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    /// <summary>
    /// لیست کد Permissionهای این نقش
    /// </summary>
    public List<string> Permissions { get; set; } = new();
}
