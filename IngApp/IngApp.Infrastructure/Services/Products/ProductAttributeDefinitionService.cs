using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Products;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Products.Attributes;
using IngApp.Application.Features.Products.DTO;
using IngApp.Domain.Entities.Products;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Products;

public class ProductAttributeDefinitionService : IProductAttributeDefinitionService
{
    private readonly AppDbContext _db;

    public ProductAttributeDefinitionService(AppDbContext db)
    {
        _db = db;
    }

    // ----------------------------------------------------
    // لیست با Paging / Filtering / Sorting
    // ----------------------------------------------------
    public async Task<PagedResult<ProductAttributeDefinitionDto>> GetPagedAsync(
        ProductAttributeDefinitionListQuery filter)
    {
        var page = filter.Page <= 0 ? 1 : filter.Page;
        var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

        var query = _db.ProductAttributeDefinitions
            .AsNoTracking()
            .AsQueryable();

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

        // مرتب‌سازی (هم‌الگوی KYC)
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
            .Select(x => new ProductAttributeDefinitionDto
            {
                Id = x.Id,
                DisplayName = x.DisplayName,
                DataType = x.DataType,
                Unit = x.Unit,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return new PagedResult<ProductAttributeDefinitionDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };
    }

    // ----------------------------------------------------
    // لیست کامل
    // ----------------------------------------------------
    public async Task<List<ProductAttributeDefinitionDto>> GetAllAsync()
    {
        return await _db.ProductAttributeDefinitions
            .AsNoTracking()
            .OrderBy(x => x.DisplayName)
            .Select(x => new ProductAttributeDefinitionDto
            {
                Id = x.Id,
                DisplayName = x.DisplayName,
                DataType = x.DataType,
                Unit = x.Unit,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<ProductAttributeDefinitionDto>> GetActiveAsync()
    {
        return await _db.ProductAttributeDefinitions
            .Where(x => x.IsActive)
            .AsNoTracking()
            .OrderBy(x => x.DisplayName)
            .Select(x => new ProductAttributeDefinitionDto
            {
                Id = x.Id,
                DisplayName = x.DisplayName,
                DataType = x.DataType,
                Unit = x.Unit,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    // ----------------------------------------------------
    // گرفتن با Id
    // ----------------------------------------------------
    public async Task<ProductAttributeDefinitionDto?> GetByIdAsync(int id)
    {
        var x = await _db.ProductAttributeDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (x == null)
            return null;

        return new ProductAttributeDefinitionDto
        {
            Id = x.Id,
            DisplayName = x.DisplayName,
            DataType = x.DataType,
            Unit = x.Unit,
            IsActive = x.IsActive
        };
    }

    // ----------------------------------------------------
    // ایجاد
    // ----------------------------------------------------
    public async Task<ProductAttributeDefinitionDto> CreateAsync(
        CreateProductAttributeDefinitionRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.DisplayName))
            errors.Add("عنوان ویژگی اجباری است.");

        if (errors.Any())
            throw new ValidationException(errors);

        var entity = new ProductAttributeDefinition
        {
            DisplayName = request.DisplayName.Trim(),
            DataType = request.DataType,
            Unit = request.Unit,
            IsActive = true
        };

        _db.ProductAttributeDefinitions.Add(entity);
        await _db.SaveChangesAsync();

        return new ProductAttributeDefinitionDto
        {
            Id = entity.Id,
            DisplayName = entity.DisplayName,
            DataType = entity.DataType,
            Unit = entity.Unit,
            IsActive = entity.IsActive
        };
    }

    // ----------------------------------------------------
    // ویرایش
    // ----------------------------------------------------
    public async Task<ProductAttributeDefinitionDto> UpdateAsync(
        int id,
        UpdateProductAttributeDefinitionRequest request)
    {
        var entity = await _db.ProductAttributeDefinitions
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            throw new NotFoundException("ویژگی مورد نظر یافت نشد.");

        if (string.IsNullOrWhiteSpace(request.DisplayName))
            throw new ValidationException(new() { "عنوان ویژگی اجباری است." });

        entity.DisplayName = request.DisplayName.Trim();
        entity.DataType = request.DataType;
        entity.Unit = request.Unit;

        await _db.SaveChangesAsync();

        return new ProductAttributeDefinitionDto
        {
            Id = entity.Id,
            DisplayName = entity.DisplayName,
            DataType = entity.DataType,
            Unit = entity.Unit,
            IsActive = entity.IsActive
        };
    }

    // ----------------------------------------------------
    // فعال / غیرفعال‌سازی
    // ----------------------------------------------------
    public async Task ActivateAsync(int id)
    {
        var entity = await _db.ProductAttributeDefinitions
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("ویژگی مورد نظر یافت نشد.");

        if (!entity.IsActive)
        {
            entity.IsActive = true;
            await _db.SaveChangesAsync();
        }
    }

    public async Task DeactivateAsync(int id)
    {
        var entity = await _db.ProductAttributeDefinitions
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("ویژگی مورد نظر یافت نشد.");

        if (entity.IsActive)
        {
            entity.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }
}
