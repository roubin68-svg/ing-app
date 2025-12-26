using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Menus;
using IngApp.Application.Features.Menus.DTO;
using IngApp.Domain.Entities.Menus;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Menus
{
    public class MenuService : IMenuService
    {
        private readonly AppDbContext _context;

        public MenuService(AppDbContext context)
        {
            _context = context;
        }

        // ======================================================
        // ===============  Dynamic Menu For User  ==============
        // ======================================================
        public async Task<List<MenuItemDto>> GetMenuForUserAsync(
            IEnumerable<string> permissions,
            IEnumerable<string> roles)
        {
            var permissionList = permissions.Distinct().ToList();
            var roleList = roles.Distinct().ToList();

            bool isAdmin = roleList.Contains("Admin");

            IQueryable<MenuItem> query = _context.MenuItems
                .Include(x => x.Children)
                .Where(x => x.IsActive);

            if (!isAdmin)
            {
                query = query.Where(x =>
                    x.RequiredPermissionCode == null ||
                    permissionList.Contains(x.RequiredPermissionCode));
            }

            var items = await query
                .OrderBy(x => x.Order)
                .AsNoTracking()
                .ToListAsync();

            return BuildTree(items, null);
        }

        private List<MenuItemDto> BuildTree(List<MenuItem> all, int? parentId)
        {
            return all
                .Where(x => x.ParentId == parentId)
                .OrderBy(x => x.Order)
                .Select(x => new MenuItemDto
                {
                    Id = x.Id,
                    Key = x.Key,
                    Title = x.Title,
                    Icon = x.Icon,
                    Route = x.Route,
                    Order = x.Order,
                    ParentId = x.ParentId,
                    RequiredPermissionCode = x.RequiredPermissionCode,
                    IsActive = x.IsActive,
                    Children = BuildTree(all, x.Id)
                })
                .ToList();
        }

        // ======================================================
        // ==================  Admin Full Menu  =================
        // ======================================================
        public async Task<List<MenuItemDto>> GetAllForAdminAsync()
        {
            var items = await _context.MenuItems
                .OrderBy(x => x.Order)
                .AsNoTracking()
                .ToListAsync();

            return BuildTree(items, null);
        }

        // ======================================================
        // ===============  Create Menu Item  ===================
        // ======================================================
        public async Task<MenuItemDto> CreateAsync(CreateMenuItemDto dto)
        {
            if (dto.ParentId.HasValue)
            {
                bool parentExists = await _context.MenuItems
                    .AnyAsync(x => x.Id == dto.ParentId.Value);

                if (!parentExists)
                    throw new NotFoundException("والد انتخاب‌شده یافت نشد.");
            }

            var item = new MenuItem
            {
                Key = dto.Key.Trim(),
                Title = dto.Title.Trim(),
                Icon = dto.Icon,
                Route = dto.Route,
                ParentId = dto.ParentId,
                Order = dto.Order,
                RequiredPermissionCode = dto.RequiredPermissionCode,
                IsActive = dto.IsActive
            };

            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();

            await NormalizeOrderAsync(item.ParentId);

            return await GetByIdAsDto(item.Id);
        }

        // ======================================================
        // ===============  Update Menu Item  ===================
        // ======================================================
        public async Task<MenuItemDto> UpdateAsync(int id, UpdateMenuItemDto dto)
        {
            var item = await _context.MenuItems.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
                throw new NotFoundException("منو یافت نشد.");

            var oldParentId = item.ParentId;

            if (dto.ParentId.HasValue)
            {
                if (dto.ParentId == id)
                    throw new ValidationException(new() { "یک منو نمی‌تواند والد خودش باشد." });

                bool parentExists = await _context.MenuItems
                    .AnyAsync(x => x.Id == dto.ParentId.Value);

                if (!parentExists)
                    throw new NotFoundException("والد انتخاب‌شده یافت نشد.");

                if (await IsDescendant(id, dto.ParentId.Value))
                    throw new ValidationException(new() { "نمی‌توان یک والد را به زیرمجموعه خود منتقل کرد." });
            }

            item.Title = dto.Title.Trim();
            item.Icon = dto.Icon;
            item.Route = dto.Route;
            item.ParentId = dto.ParentId;
            item.Order = dto.Order;
            item.RequiredPermissionCode = dto.RequiredPermissionCode;
            item.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            if (oldParentId != item.ParentId)
                await NormalizeOrderAsync(oldParentId);

            await NormalizeOrderAsync(item.ParentId);

            return await GetByIdAsDto(id);
        }

        // ======================================================
        // ===============  Delete Menu Item  ===================
        // ======================================================
        public async Task DeleteAsync(int id)
        {
            var item = await _context.MenuItems
                .Include(x => x.Children)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
                throw new NotFoundException("منو یافت نشد.");

            if (item.Children.Any())
                throw new AppException("ابتدا زیرمنوهای این منو باید حذف شوند.");

            var parentId = item.ParentId;

            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();

            await NormalizeOrderAsync(parentId);
        }

        // ======================================================
        // =============  Change Menu Parent  ===================
        // ======================================================
        public async Task ChangeParentAsync(int id, int? parentId)
        {
            var item = await _context.MenuItems.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
                throw new NotFoundException("منو یافت نشد.");

            var oldParentId = item.ParentId;

            if (parentId == id)
                throw new ValidationException(new() { "یک منو نمی‌تواند والد خودش باشد." });

            if (parentId.HasValue)
            {
                var parentExists = await _context.MenuItems
                    .AnyAsync(x => x.Id == parentId.Value);

                if (!parentExists)
                    throw new NotFoundException("والد انتخاب‌شده یافت نشد.");

                if (await IsDescendant(id, parentId.Value))
                    throw new ValidationException(new() { "نمی‌توان یک والد را به زیرمجموعه خود منتقل کرد." });
            }

            item.ParentId = parentId;
            await _context.SaveChangesAsync();

            if (oldParentId != parentId)
                await NormalizeOrderAsync(oldParentId);

            await NormalizeOrderAsync(parentId);
        }

        // ======================================================
        // =============  Change Menu Order  ====================
        // ======================================================
        public async Task ChangeOrderAsync(int id, int newOrder)
        {
            var item = await _context.MenuItems.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
                throw new NotFoundException("منو یافت نشد.");

            item.Order = newOrder;
            await _context.SaveChangesAsync();

            await NormalizeOrderAsync(item.ParentId);
        }

        // ======================================================
        // ===========  Change Required Permission  =============
        // ======================================================
        public async Task ChangePermissionAsync(int id, string? permissionCode)
        {
            var item = await _context.MenuItems.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
                throw new NotFoundException("منو یافت نشد.");

            item.RequiredPermissionCode = permissionCode;
            await _context.SaveChangesAsync();
        }

        // ======================================================
        // ================  Change Status  ======================
        // ======================================================
        public async Task ChangeStatusAsync(int id, bool isActive)
        {
            var item = await _context.MenuItems.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
                throw new NotFoundException("منو یافت نشد.");

            item.IsActive = isActive;
            await _context.SaveChangesAsync();
        }

        // ======================================================
        // ===============  Helpers  =============================
        // ======================================================

        /// <summary>
        /// مرتب‌سازی Order برای همهٔ فرزندان یک والد (null = ریشه)
        /// </summary>
        private async Task NormalizeOrderAsync(int? parentId)
        {
            var siblings = await _context.MenuItems
                .Where(m => m.ParentId == parentId)
                .OrderBy(m => m.Order)
                .ThenBy(m => m.Id)
                .ToListAsync();

            for (int i = 0; i < siblings.Count; i++)
            {
                siblings[i].Order = i + 1;
            }

            if (siblings.Count > 0)
                await _context.SaveChangesAsync();
        }

        private async Task<bool> IsDescendant(int targetId, int parentId)
        {
            var children = await _context.MenuItems
                .Where(x => x.ParentId == targetId)
                .Select(x => x.Id)
                .ToListAsync();

            foreach (var child in children)
            {
                if (child == parentId)
                    return true;

                if (await IsDescendant(child, parentId))
                    return true;
            }

            return false;
        }

        private async Task<MenuItemDto> GetByIdAsDto(int id)
        {
            var item = await _context.MenuItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
                throw new NotFoundException("منو یافت نشد.");

            return new MenuItemDto
            {
                Id = item.Id,
                Key = item.Key,
                Title = item.Title,
                Icon = item.Icon,
                Route = item.Route,
                Order = item.Order,
                ParentId = item.ParentId,
                RequiredPermissionCode = item.RequiredPermissionCode,
                IsActive = item.IsActive,
                Children = new List<MenuItemDto>()
            };
        }
    }
}
