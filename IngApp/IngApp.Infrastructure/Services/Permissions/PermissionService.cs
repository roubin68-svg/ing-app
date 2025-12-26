using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Permissions;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Permissions.DTO;
using IngApp.Domain.Entities.Permissions;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Permissions
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;

        public PermissionService(AppDbContext context)
        {
            _context = context;
        }

        // ================================================
        // CREATE
        // ================================================
        public async Task<Guid> CreatePermissionAsync(CreatePermissionRequest request)
        {
            var exists = await _context.Permissions.AnyAsync(x => x.Code == request.Code.Trim());
            if (exists)
                throw new ValidationException(new() { $"مجوز با کد '{request.Code}' قبلاً ثبت شده است." });

            var entity = new Permission
            {
                Id = Guid.NewGuid(),
                Code = request.Code.Trim(),
                DisplayName = request.DisplayName.Trim(),
                Description = request.Description?.Trim(),
                IsActive = true
            };

            _context.Permissions.Add(entity);
            await _context.SaveChangesAsync();

            return entity.Id;
        }

        // ================================================
        // GET ALL
        // ================================================
        public async Task<List<PermissionDto>> GetAllPermissionsAsync()
        {
            return await _context.Permissions
                .AsNoTracking()
                .OrderBy(x => x.Code)
                .Select(x => new PermissionDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    DisplayName = x.DisplayName,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }

        // ================================================
        // GET BY ID
        // ================================================
        public async Task<PermissionDto> GetPermissionByIdAsync(Guid id)
        {
            var p = await _context.Permissions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (p == null)
                throw new NotFoundException("مجوز یافت نشد.");

            return new PermissionDto
            {
                Id = p.Id,
                Code = p.Code,
                DisplayName = p.DisplayName,
                Description = p.Description,
                IsActive = p.IsActive
            };
        }

        // ================================================
        // UPDATE
        // ================================================
        public async Task<PermissionDto> UpdatePermissionAsync(Guid id, UpdatePermissionRequest request)
        {
            var entity = await _context.Permissions.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new NotFoundException("مجوز یافت نشد.");

            entity.DisplayName = request.DisplayName.Trim();
            entity.Description = request.Description?.Trim();
            entity.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return new PermissionDto
            {
                Id = entity.Id,
                Code = entity.Code,
                DisplayName = entity.DisplayName,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }

        // ================================================
        // DELETE
        // ================================================
        public async Task DeletePermissionAsync(Guid id)
        {
            var entity = await _context.Permissions
                .Include(x => x.RolePermissions)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new NotFoundException("مجوز یافت نشد.");

            if (entity.RolePermissions.Any())
                throw new AppException("امکان حذف مجوزی که به نقش متصل است وجود ندارد.");

            _context.Permissions.Remove(entity);
            await _context.SaveChangesAsync();
        }

        // ================================================
        // PAGING / FILTER / SORT
        // ================================================
        public async Task<PagedResult<PermissionDto>> GetPagedPermissionsAsync(PermissionListQuery request)
        {
            if (request.Page <= 0) request.Page = 1;
            if (request.PageSize <= 0) request.PageSize = 10;

            var query = _context.Permissions.AsNoTracking();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.Code.ToLower().Contains(search) ||
                    x.DisplayName.ToLower().Contains(search));
            }

            // Active filter
            if (request.IsActive.HasValue)
                query = query.Where(x => x.IsActive == request.IsActive.Value);

            // Sorting
            var sortBy = request.SortBy?.Trim().ToLower();
            var sortDesc = request.SortDesc;

            query = sortBy switch
            {
                "displayname" => sortDesc
                    ? query.OrderByDescending(x => x.DisplayName)
                    : query.OrderBy(x => x.DisplayName),

                "isactive" => sortDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => sortDesc
                    ? query.OrderByDescending(x => x.Code)
                    : query.OrderBy(x => x.Code)
            };

            var totalCount = await query.CountAsync();
            var skip = (request.Page - 1) * request.PageSize;

            var items = await query
                .Skip(skip)
                .Take(request.PageSize)
                .Select(x => new PermissionDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    DisplayName = x.DisplayName,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return new PagedResult<PermissionDto>
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

        // ================================================
        // GET ROLES FOR PERMISSION
        // ================================================
        public async Task<List<PermissionRoleDto>> GetRolesByPermissionIdAsync(Guid permissionId)
        {
            var exists = await _context.Permissions.AnyAsync(x => x.Id == permissionId);
            if (!exists)
                throw new NotFoundException("مجوز یافت نشد.");

            return await _context.RolePermissions
                .Where(rp => rp.PermissionId == permissionId)
                .Select(rp => new PermissionRoleDto
                {
                    Id = rp.Role!.Id,
                    Name = rp.Role.Name,
                    DisplayName = rp.Role.DisplayName
                })
                .ToListAsync();
        }
    }
}
