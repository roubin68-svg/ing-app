namespace IngApp.Application.Features.Menus.DTO
{
    public class CreateMenuItemDto
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Route { get; set; }

        public int? ParentId { get; set; }
        public int Order { get; set; }

        public string? RequiredPermissionCode { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
