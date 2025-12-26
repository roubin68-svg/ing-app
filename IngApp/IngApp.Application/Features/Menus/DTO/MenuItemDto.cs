namespace IngApp.Application.Features.Menus.DTO
{
    public class MenuItemDto
    {
        public int Id { get; set; }

        public string Key { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        // مسیر/URL صفحه — باید دقیقا مطابق MenuService باشد
        public string? Route { get; set; }

        public string? Icon { get; set; }

        public int Order { get; set; }

        public int? ParentId { get; set; }

        public bool IsActive { get; set; }

        public string? RequiredPermissionCode { get; set; }

        public List<MenuItemDto> Children { get; set; } = new();
    }
}
