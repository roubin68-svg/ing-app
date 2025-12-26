namespace IngApp.Domain.Entities.Menus;

public class MenuItem
{
    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string? Icon { get; set; }

    public int? ParentId { get; set; }
    public MenuItem? Parent { get; set; }

    public ICollection<MenuItem> Children { get; set; } = new List<MenuItem>();

    public int Order { get; set; }

    public bool IsActive { get; set; } = true;

    // null → برای همه کاربران لاگین‌کرده
    public string? RequiredPermissionCode { get; set; }
}
