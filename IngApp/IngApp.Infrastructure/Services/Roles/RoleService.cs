using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Roles;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Roles.DTO;
using IngApp.Domain.Entities;
using IngApp.Domain.Entities.Roles;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Roles
{
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _context;

        public RoleService(AppDbContext context)
        {
            _context = context;
        }

        // ========================================================
        // CREATE
        // ========================================================
        public async Task<RoleDto> CreateAsync(CreateRoleDto dto)
        {
            var exists = await _context.Roles
                .AnyAsync(r => r.Name == dto.Name.Trim());

            if (exists)
                throw new ValidationException(new() { "نقشی با این نام از قبل وجود دارد." });

            var role = new Role
            {
                Name = dto.Name.Trim(),
                DisplayName = dto.DisplayName.Trim(),
                Description = dto.Description?.Trim(),
                IsActive = true
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var result = await MapToDtoAsync(role.Id);

            if (result == null)
                throw new AppException("خطا در واکشی نقش بعد از ایجاد.");

            return result;
        }

        // ========================================================
        // UPDATE
        // ========================================================
        public async Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto dto)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
            if (role == null)
                throw new NotFoundException("نقش یافت نشد.");

            role.DisplayName = dto.DisplayName.Trim();
            role.Description = dto.Description?.Trim();
            role.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            var updated = await MapToDtoAsync(role.Id);
            if (updated == null)
                throw new AppException("خطا در واکشی نقش بعد از ویرایش.");

            return updated;
        }

        // ========================================================
        // DELETE
        // ========================================================
        public async Task DeleteAsync(Guid id)
        {
            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                throw new NotFoundException("نقش یافت نشد.");

            if (role.UserRoles.Any())
                throw new AppException("امکان حذف نقشی که به کاربر اختصاص داده شده وجود ندارد.");

            _context.RolePermissions.RemoveRange(role.RolePermissions);
            _context.Roles.Remove(role);

            await _context.SaveChangesAsync();
        }

        // ========================================================
        // ASSIGN PERMISSIONS
        // ========================================================
        public async Task AssignPermissionsAsync(Guid id, AssignPermissionsToRoleDto dto)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                throw new NotFoundException("نقش یافت نشد.");

            var requestedCodes = dto.PermissionCodes
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();

            // اگر هیچ Permission ارسال نشد → حذف همه
            if (!requestedCodes.Any())
            {
                _context.RolePermissions.RemoveRange(role.RolePermissions);
                await _context.SaveChangesAsync();
                return;
            }

            // لیست Permission های موجود
            var permissions = await _context.Permissions
                .Where(p => requestedCodes.Contains(p.Code))
                .ToListAsync();

            // چک کردن کدهای ناموجود
            var foundCodes = permissions.Select(p => p.Code).ToHashSet();
            var missingCodes = requestedCodes.Where(c => !foundCodes.Contains(c)).ToList();

            if (missingCodes.Any())
                throw new ValidationException(new() { $"مجوزهای زیر یافت نشدند: {string.Join(", ", missingCodes)}" });

            // حذف Permission های قدیمی
            var toRemove = role.RolePermissions
                .Where(rp => !requestedCodes.Contains(rp.Permission!.Code))
                .ToList();

            _context.RolePermissions.RemoveRange(toRemove);

            // اضافه کردن Permission های جدید
            var existingCodes = role.RolePermissions.Select(rp => rp.Permission!.Code).ToHashSet();

            foreach (var perm in permissions)
            {
                if (!existingCodes.Contains(perm.Code))
                {
                    role.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = perm.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        // ========================================================
        // GET ALL
        // ========================================================
        public async Task<List<RoleDto>> GetAllAsync()
        {
            var roles = await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .AsNoTracking()
                .ToListAsync();

            return roles.Select(MapToDto).ToList();
        }

        // ========================================================
        // GET BY ID
        // ========================================================
        public async Task<RoleDto> GetByIdAsync(Guid id)
        {
            var dto = await MapToDtoAsync(id);
            if (dto == null)
                throw new NotFoundException("نقش یافت نشد.");

            return dto;
        }

        // ========================================================
        // PAGED
        // ========================================================
        public async Task<PagedResult<RoleDto>> GetPagedAsync(RoleListQuery query)
        {
            if (query.Page <= 0) query.Page = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            var rolesQuery = _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .AsNoTracking();

            // Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                rolesQuery = rolesQuery.Where(r =>
                    r.Name.ToLower().Contains(search) ||
                    r.DisplayName.ToLower().Contains(search));
            }

            // Active
            if (query.IsActive.HasValue)
                rolesQuery = rolesQuery.Where(r => r.IsActive == query.IsActive.Value);

            // Sorting
            var sortBy = query.SortBy?.Trim().ToLowerInvariant();
            var sortDesc = query.SortDesc;

            rolesQuery = sortBy switch
            {
                "displayname" => sortDesc
                    ? rolesQuery.OrderByDescending(r => r.DisplayName).ThenBy(r => r.Name)
                    : rolesQuery.OrderBy(r => r.DisplayName).ThenBy(r => r.Name),

                "isactive" => sortDesc
                    ? rolesQuery.OrderByDescending(r => r.IsActive).ThenBy(r => r.Name)
                    : rolesQuery.OrderBy(r => r.IsActive).ThenBy(r => r.Name),

                _ => sortDesc
                    ? rolesQuery.OrderByDescending(r => r.Name)
                    : rolesQuery.OrderBy(r => r.Name)
            };

            var totalCount = await rolesQuery.CountAsync();
            var skip = (query.Page - 1) * query.PageSize;

            var items = await rolesQuery.Skip(skip).Take(query.PageSize).ToListAsync();
            var dtoItems = items.Select(MapToDto).ToList();

            return new PagedResult<RoleDto>
            {
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                Items = dtoItems
            };
        }

        // ========================================================
        // HELPERS
        // ========================================================
        private async Task<RoleDto?> MapToDtoAsync(Guid id)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            return role == null ? null : MapToDto(role);
        }

        private static RoleDto MapToDto(Role role)
        {
            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                DisplayName = role.DisplayName,
                Description = role.Description,
                IsActive = role.IsActive,
                Permissions = role.RolePermissions
                    .Select(rp => rp.Permission!.Code)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList()
            };
        }
    }
}
