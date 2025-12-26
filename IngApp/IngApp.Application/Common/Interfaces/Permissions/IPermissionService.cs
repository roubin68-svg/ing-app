using IngApp.Application.Common.Models;
using IngApp.Application.Features.Permissions.DTO;

namespace IngApp.Application.Common.Interfaces.Permissions
{
    public interface IPermissionService
    {
        Task<Guid> CreatePermissionAsync(CreatePermissionRequest request);

        Task<List<PermissionDto>> GetAllPermissionsAsync();

        Task<PermissionDto?> GetPermissionByIdAsync(Guid id);

        Task<PermissionDto> UpdatePermissionAsync(Guid id, UpdatePermissionRequest request);

        Task DeletePermissionAsync(Guid id);

        // ✅ متد جدید برای Paging + Filter + Sort
        Task<PagedResult<PermissionDto>> GetPagedPermissionsAsync(PermissionListQuery request);
        Task<List<PermissionRoleDto>> GetRolesByPermissionIdAsync(Guid permissionId);


    }
}
