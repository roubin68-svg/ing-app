using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Products;
using IngApp.Application.Features.Products.DTO;
using IngApp.Domain.Entities.Products;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Products;

public class ProductCategoryService : IProductCategoryService
{
    private readonly AppDbContext _context;

    public ProductCategoryService(AppDbContext context)
    {
        _context = context;
    }

    // ======================================================
    // ==================  Get All  =========================
    // ======================================================
    public async Task<List<ProductCategoryDto>> GetAllAsync()
    {
        return await _context.ProductCategories
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new ProductCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ParentId = x.ParentId,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    // ======================================================
    // ==================  Create  ==========================
    // ======================================================
    public async Task<ProductCategoryDto> CreateAsync(CreateProductCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException(new() { "نام دسته‌بندی الزامی است." });

        if (request.ParentId.HasValue)
        {
            var parentExists = await _context.ProductCategories
                .AnyAsync(x => x.Id == request.ParentId.Value);

            if (!parentExists)
                throw new NotFoundException("دسته‌بندی والد یافت نشد.");
        }

        var duplicate = await _context.ProductCategories
            .AnyAsync(x => x.Name == request.Name.Trim() && x.ParentId == request.ParentId);

        if (duplicate)
            throw new ValidationException(new() { "دسته‌بندی با این نام در این سطح قبلاً ثبت شده است." });

        var category = new ProductCategory
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            ParentId = request.ParentId,
            IsActive = true
        };

        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync();

        return await GetByIdAsDto(category.Id);
    }

    // ======================================================
    // ==================  Update  ==========================
    // ======================================================
    public async Task<ProductCategoryDto> UpdateAsync(int id, UpdateProductCategoryRequest request)
    {
        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (category == null)
            throw new NotFoundException("دسته‌بندی مورد نظر یافت نشد.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException(new() { "نام دسته‌بندی الزامی است." });

        if (request.ParentId == id)
            throw new ValidationException(new() { "یک دسته‌بندی نمی‌تواند والد خودش باشد." });

        if (request.ParentId.HasValue)
        {
            var parentExists = await _context.ProductCategories
                .AnyAsync(x => x.Id == request.ParentId.Value);

            if (!parentExists)
                throw new NotFoundException("دسته‌بندی والد یافت نشد.");

            if (await IsDescendant(id, request.ParentId.Value))
                throw new ValidationException(new() { "نمی‌توان یک دسته‌بندی را زیرمجموعه فرزند خودش قرار داد." });
        }

        var duplicate = await _context.ProductCategories
            .AnyAsync(x =>
                x.Id != id &&
                x.Name == request.Name.Trim() &&
                x.ParentId == request.ParentId);

        if (duplicate)
            throw new ValidationException(new() { "دسته‌بندی با این نام در این سطح قبلاً ثبت شده است." });

        category.Name = request.Name.Trim();
        category.Description = request.Description;
        category.ParentId = request.ParentId;

        await _context.SaveChangesAsync();

        return await GetByIdAsDto(id);
    }

    // ======================================================
    // ==================  Activate  ========================
    // ======================================================
    public async Task ActivateAsync(int id)
    {
        var category = await _context.ProductCategories.FindAsync(id);

        if (category == null)
            throw new NotFoundException("دسته‌بندی مورد نظر یافت نشد.");

        if (category.IsActive)
            throw new AppException("دسته‌بندی در حال حاضر فعال است.");

        category.IsActive = true;
        await _context.SaveChangesAsync();
    }

    // ======================================================
    // ==================  Deactivate  ======================
    // ======================================================
    public async Task DeactivateAsync(int id)
    {
        var category = await _context.ProductCategories.FindAsync(id);

        if (category == null)
            throw new NotFoundException("دسته‌بندی مورد نظر یافت نشد.");

        var hasChildren = await _context.ProductCategories
            .AnyAsync(x => x.ParentId == id && x.IsActive);

        if (hasChildren)
            throw new AppException("ابتدا باید زیر‌دسته‌های فعال این دسته‌بندی غیرفعال شوند.");

        if (!category.IsActive)
            throw new AppException("دسته‌بندی در حال حاضر غیرفعال است.");

        category.IsActive = false;
        await _context.SaveChangesAsync();
    }

    // ======================================================
    // ==================  Helpers  =========================
    // ======================================================
    private async Task<bool> IsDescendant(int targetId, int parentId)
    {
        var childrenIds = await _context.ProductCategories
            .Where(x => x.ParentId == targetId)
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var childId in childrenIds)
        {
            if (childId == parentId)
                return true;

            if (await IsDescendant(childId, parentId))
                return true;
        }

        return false;
    }

    private async Task<ProductCategoryDto> GetByIdAsDto(int id)
    {
        var category = await _context.ProductCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (category == null)
            throw new NotFoundException("دسته‌بندی مورد نظر یافت نشد.");

        return new ProductCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentId = category.ParentId,
            IsActive = category.IsActive
        };
    }
}
