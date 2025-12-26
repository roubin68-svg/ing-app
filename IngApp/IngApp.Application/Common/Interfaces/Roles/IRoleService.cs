// IngApp.Application/Common/Interfaces/Roles/IRoleService.cs
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Roles.DTO;

namespace IngApp.Application.Common.Interfaces.Roles
{
    public interface IRoleService
    {
        Task<RoleDto> CreateAsync(CreateRoleDto dto);
        Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto dto);
        Task DeleteAsync(Guid id);
        Task AssignPermissionsAsync(Guid id, AssignPermissionsToRoleDto dto);
        Task<List<RoleDto>> GetAllAsync();
        Task<RoleDto?> GetByIdAsync(Guid id);

        // ✅ متد جدید برای صفحه‌بندی نقش‌ها
        Task<PagedResult<RoleDto>> GetPagedAsync(RoleListQuery query);
    }
}
