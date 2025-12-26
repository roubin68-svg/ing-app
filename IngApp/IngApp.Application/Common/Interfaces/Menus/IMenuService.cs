using IngApp.Application.Features.Menus.DTO;

namespace IngApp.Application.Common.Interfaces.Menus
{
    public interface IMenuService
    {
        // ===========================
        // Dynamic Menu For Logged-in User
        // ===========================
        Task<List<MenuItemDto>> GetMenuForUserAsync(
            IEnumerable<string> permissions,
            IEnumerable<string> roles);

        // ===========================
        // Admin Full Menu (Tree)
        // ===========================
        Task<List<MenuItemDto>> GetAllForAdminAsync();

        // ===========================
        // CRUD
        // ===========================
        Task<MenuItemDto> CreateAsync(CreateMenuItemDto dto);

        Task<MenuItemDto> UpdateAsync(int id, UpdateMenuItemDto dto);

        Task DeleteAsync(int id);

        // ===========================
        // Menu Modifications
        // ===========================
        Task ChangeOrderAsync(int id, int newOrder);

        Task ChangeParentAsync(int id, int? parentId);

        Task ChangePermissionAsync(int id, string? permissionCode);

        Task ChangeStatusAsync(int id, bool isActive);
    }
}
