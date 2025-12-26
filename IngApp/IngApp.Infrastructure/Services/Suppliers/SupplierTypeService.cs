using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Suppliers;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Suppliers.DTO;
using IngApp.Domain.Entities.Suppliers;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Suppliers
{
    public class SupplierTypeService : ISupplierTypeService
    {
        private readonly AppDbContext _db;

        public SupplierTypeService(AppDbContext db)
        {
            _db = db;
        }

        // ----------------------------------------------------
        // Paging + Filtering + Sorting
        // ----------------------------------------------------
        public async Task<PagedResult<SupplierTypeDto>> GetPagedAsync(SupplierTypeListQueryDto filter)
        {
            var page = filter.Page <= 0 ? 1 : filter.Page;
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            var query = _db.SupplierTypes.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var term = filter.Name.Trim();
                // جستجو روی Name و Description
                query = query.Where(x =>
                    x.Name.Contains(term) ||
                    (x.Description != null && x.Description.Contains(term)));
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == filter.IsActive.Value);
            }

            var sortBy = (filter.SortBy ?? string.Empty).ToLowerInvariant();
            var desc = filter.SortDesc;

            query = sortBy switch
            {
                "name" =>
                    desc ? query.OrderByDescending(x => x.Name)
                         : query.OrderBy(x => x.Name),
                _ =>
                    desc ? query.OrderByDescending(x => x.Id)
                         : query.OrderBy(x => x.Id)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SupplierTypeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return new PagedResult<SupplierTypeDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

        // ----------------------------------------------------
        // لیست کامل (برای DropDown / فرم‌ها)
        // ----------------------------------------------------
        public async Task<List<SupplierTypeDto>> GetAllAsync()
        {
            return await _db.SupplierTypes
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new SupplierTypeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }

        public async Task<SupplierTypeDto?> GetByIdAsync(int id)
        {
            var entity = await _db.SupplierTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return null;

            return new SupplierTypeDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }

        // ----------------------------------------------------
        // ایجاد
        // ----------------------------------------------------
        public async Task<SupplierTypeDto> CreateAsync(CreateSupplierTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException(new() { "نام نوع تأمین‌کننده اجباری است." });

            var name = request.Name.Trim();
            var description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();

            var exists = await _db.SupplierTypes
                .AnyAsync(x => x.Name == name);

            if (exists)
                throw new ValidationException(new()
                {
                    $"نوع تأمین‌کننده با نام \"{name}\" قبلاً ثبت شده است."
                });

            var entity = new SupplierType
            {
                Name = name,
                Description = description,
                IsActive = true
            };

            _db.SupplierTypes.Add(entity);
            await _db.SaveChangesAsync();

            return new SupplierTypeDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }

        // ----------------------------------------------------
        // ویرایش
        // ----------------------------------------------------
        public async Task<SupplierTypeDto> UpdateAsync(int id, UpdateSupplierTypeRequest request)
        {
            var entity = await _db.SupplierTypes
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException("نوع تأمین‌کننده یافت نشد.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException(new() { "نام نوع تأمین‌کننده اجباری است." });

            var name = request.Name.Trim();
            var description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();

            var exists = await _db.SupplierTypes
                .AnyAsync(x => x.Id != id && x.Name == name);

            if (exists)
                throw new ValidationException(new()
                {
                    $"نوع تأمین‌کننده با نام \"{name}\" قبلاً ثبت شده است."
                });

            entity.Name = name;
            entity.Description = description;

            await _db.SaveChangesAsync();

            return new SupplierTypeDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }

        // ----------------------------------------------------
        // فعال / غیرفعال‌سازی
        // ----------------------------------------------------
        public async Task ActivateAsync(int id)
        {
            var entity = await _db.SupplierTypes
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException("نوع تأمین‌کننده یافت نشد.");

            if (!entity.IsActive)
            {
                entity.IsActive = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeactivateAsync(int id)
        {
            var entity = await _db.SupplierTypes
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException("نوع تأمین‌کننده یافت نشد.");

            // اگر SupplierProfile با این نوع وجود دارد، اجازه غیرفعالسازی نمی‌دهیم
            var hasSuppliers = await _db.SupplierProfiles
                .AnyAsync(x => x.SupplierTypeId == id);

            if (hasSuppliers)
            {
                throw new AppException(
                    "به دلیل استفاده برخی تأمین‌کنندگان از این نوع، امکان غیرفعالسازی وجود ندارد.");
            }

            if (entity.IsActive)
            {
                entity.IsActive = false;
                await _db.SaveChangesAsync();
            }
        }
    }
}
