using IngApp.Application.Common.Models;
using IngApp.Application.Features.Users.DTO;

namespace IngApp.Application.Common.Interfaces.Users
{
    public interface IUserService
    {
        // لیست کامل (در صورت نیاز)
        Task<List<UserDto>> GetAllAsync();

        // لیست صفحه‌بندی شده
        Task<PagedResult<UserDto>> GetPagedAsync(UserListQueryDto filter);

        // گرفتن یک کاربر
        Task<UserDto?> GetByIdAsync(Guid id);

        // ساخت کاربر جدید
        Task<UserDto> CreateAsync(CreateUserDto dto);

        // ویرایش کاربر
        Task UpdateUserAsync(Guid userId, UpdateUserDto dto);

        // تغییر وضعیت فعال/غیرفعال
        Task ChangeStatusAsync(Guid userId, ChangeUserStatusDto dto);

        // نقش‌ها
        Task AssignRoleAsync(Guid userId, Guid roleId);
        Task RemoveRoleAsync(Guid userId, Guid roleId);
    }
}
