using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Kyc;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Kyc.DTO;
using IngApp.Domain.Entities.Kyc;
using IngApp.Domain.Enums;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Kyc
{
    public class KycAttributeDefinitionService : IKycAttributeDefinitionService
    {
        private readonly AppDbContext _db;

        public KycAttributeDefinitionService(AppDbContext db)
        {
            _db = db;
        }

        // ----------------------------------------------------
        // لیست با Paging / Filtering / Sorting
        // ----------------------------------------------------
        public async Task<PagedResult<KycAttributeDefinitionDto>> GetPagedAsync(KycAttributeDefinitionListQueryDto filter)
        {
            var page = filter.Page <= 0 ? 1 : filter.Page;
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            var query = _db.KycAttributeDefinitions.AsNoTracking().AsQueryable();

            // فیلترها
            if (!string.IsNullOrWhiteSpace(filter.DisplayName))
            {
                var name = filter.DisplayName.Trim();
                query = query.Where(x => x.DisplayName.Contains(name));
            }

            if (filter.DataType.HasValue)
            {
                query = query.Where(x => x.DataType == filter.DataType.Value);
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == filter.IsActive.Value);
            }

            // مرتب‌سازی
            var sortBy = (filter.SortBy ?? string.Empty).ToLowerInvariant();
            var desc = filter.SortDesc;

            query = sortBy switch
            {
                "displayname" =>
                    desc ? query.OrderByDescending(x => x.DisplayName)
                         : query.OrderBy(x => x.DisplayName),

                _ =>
                    desc ? query.OrderByDescending(x => x.Id)
                         : query.OrderBy(x => x.Id)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new KycAttributeDefinitionDto
                {
                    Id = x.Id,
                    DisplayName = x.DisplayName,
                    Description = x.Description,
                    DataType = x.DataType,
                    DefaultRequired = x.DefaultRequired,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return new PagedResult<KycAttributeDefinitionDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

        // ----------------------------------------------------
        // لیست کامل (مثلاً برای Dropdown‌ها)
        // ----------------------------------------------------
        public async Task<List<KycAttributeDefinitionDto>> GetAllAsync()
        {
            return await _db.KycAttributeDefinitions
                .AsNoTracking()
                .OrderBy(x => x.DisplayName)
                .Select(x => new KycAttributeDefinitionDto
                {
                    Id = x.Id,
                    DisplayName = x.DisplayName,
                    Description = x.Description,
                    DataType = x.DataType,
                    DefaultRequired = x.DefaultRequired,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }

        public async Task<List<KycAttributeDefinitionDto>> GetActiveAsync()
        {
            return await _db.KycAttributeDefinitions
                .Where(x => x.IsActive)
                .AsNoTracking()
                .OrderBy(x => x.DisplayName)
                .Select(x => new KycAttributeDefinitionDto
                {
                    Id = x.Id,
                    DisplayName = x.DisplayName,
                    Description = x.Description,
                    DataType = x.DataType,
                    DefaultRequired = x.DefaultRequired,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }

        // ----------------------------------------------------
        // گرفتن با Id
        // ----------------------------------------------------
        public async Task<KycAttributeDefinitionDto?> GetByIdAsync(int id)
        {
            var x = await _db.KycAttributeDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (x == null)
                return null;

            return new KycAttributeDefinitionDto
            {
                Id = x.Id,
                DisplayName = x.DisplayName,
                Description = x.Description,
                DataType = x.DataType,
                DefaultRequired = x.DefaultRequired,
                IsActive = x.IsActive
            };
        }

        // ----------------------------------------------------
        // ایجاد
        // ----------------------------------------------------
        public async Task<KycAttributeDefinitionDto> CreateAsync(CreateKycAttributeDefinitionRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.DisplayName))
                errors.Add("عنوان نمایشی اجباری است.");

            if (errors.Any())
                throw new ValidationException(errors);

            var entity = new KycAttributeDefinition
            {
                DisplayName = request.DisplayName.Trim(),
                Description = request.Description,
                DataType = request.DataType,
                DefaultRequired = request.DefaultRequired,
                IsActive = true
            };

            _db.KycAttributeDefinitions.Add(entity);
            await _db.SaveChangesAsync();

            return new KycAttributeDefinitionDto
            {
                Id = entity.Id,
                DisplayName = entity.DisplayName,
                Description = entity.Description,
                DataType = entity.DataType,
                DefaultRequired = entity.DefaultRequired,
                IsActive = entity.IsActive
            };
        }

        // ----------------------------------------------------
        // ویرایش
        // ----------------------------------------------------
        public async Task<KycAttributeDefinitionDto> UpdateAsync(int id, UpdateKycAttributeDefinitionRequest request)
        {
            var entity = await _db.KycAttributeDefinitions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new NotFoundException("فیلد مورد نظر یافت نشد.");

            if (string.IsNullOrWhiteSpace(request.DisplayName))
                throw new ValidationException(new() { "عنوان نمایشی اجباری است." });

            entity.DisplayName = request.DisplayName.Trim();
            entity.Description = request.Description;
            entity.DataType = request.DataType;
            entity.DefaultRequired = request.DefaultRequired;

            await _db.SaveChangesAsync();

            return new KycAttributeDefinitionDto
            {
                Id = entity.Id,
                DisplayName = entity.DisplayName,
                Description = entity.Description,
                DataType = entity.DataType,
                DefaultRequired = entity.DefaultRequired,
                IsActive = entity.IsActive
            };
        }

        // ----------------------------------------------------
        // فعال / غیرفعال‌سازی
        // ----------------------------------------------------
        public async Task ActivateAsync(int id)
        {
            var entity = await _db.KycAttributeDefinitions
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException("فیلد مورد نظر یافت نشد.");

            if (!entity.IsActive)
            {
                entity.IsActive = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeactivateAsync(int id)
        {
            var entity = await _db.KycAttributeDefinitions
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException("فیلد مورد نظر یافت نشد.");

            if (entity.IsActive)
            {
                entity.IsActive = false;
                await _db.SaveChangesAsync();
            }
        }
    }
}
