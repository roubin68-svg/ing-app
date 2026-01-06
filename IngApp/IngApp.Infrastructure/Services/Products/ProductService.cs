using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Products;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Products.DTO;
using IngApp.Domain.Entities.Products;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Products;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    // =====================================================
    // CREATE
    // =====================================================
    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException(new() { "نام محصول الزامی است." });

        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(x => x.Id == request.CategoryId);

        if (category == null)
            throw new NotFoundException("دسته‌بندی محصول یافت نشد.");

        if (!category.IsActive)
            throw new AppException("دسته‌بندی انتخاب‌شده غیرفعال است.");

        if (string.IsNullOrWhiteSpace(request.Unit))
            throw new ValidationException(new() { "واحد محصول الزامی است." });

        var exists = await _context.Products
            .AnyAsync(x =>
                x.Name == request.Name.Trim() &&
                x.CategoryId == request.CategoryId);

        if (exists)
            throw new ValidationException(new() { "محصولی با این نام در این دسته‌بندی قبلاً ثبت شده است." });

        var product = new Product
        {
            Name = request.Name.Trim(),
            CategoryId = request.CategoryId,
            Unit = request.Unit?.Trim(),
            ImagePath = request.ImagePath,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(product.Id);
    }

    // =====================================================
    // UPDATE
    // =====================================================
    public async Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product == null)
            throw new NotFoundException("محصول مورد نظر یافت نشد.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException(new() { "نام محصول الزامی است." });

        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(x => x.Id == request.CategoryId);

        if (category == null)
            throw new NotFoundException("دسته‌بندی محصول یافت نشد.");

        if (!category.IsActive)
            throw new AppException("دسته‌بندی انتخاب‌شده غیرفعال است.");

        if (string.IsNullOrWhiteSpace(request.Unit))
            throw new ValidationException(new() { "واحد محصول الزامی است." });

        var duplicate = await _context.Products
            .AnyAsync(x =>
                x.Id != id &&
                x.Name == request.Name.Trim() &&
                x.CategoryId == request.CategoryId);

        if (duplicate)
            throw new ValidationException(new() { "محصولی با این نام در این دسته‌بندی قبلاً ثبت شده است." });

        product.Name = request.Name.Trim();
        product.CategoryId = request.CategoryId;
        product.Unit = request.Unit?.Trim();
        product.ImagePath = request.ImagePath;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    // =====================================================
    // GET BY ID
    // =====================================================
    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product == null)
            throw new NotFoundException("محصول مورد نظر یافت نشد.");

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            Unit = product.Unit,
            ImagePath = product.ImagePath,
            IsActive = product.IsActive
        };
    }

    // =====================================================
    // PAGED
    // =====================================================
    public async Task<PagedResult<ProductDto>> GetPagedAsync(ProductListQuery query)
    {
        if (query.Page <= 0) query.Page = 1;
        if (query.PageSize <= 0) query.PageSize = 10;

        var productsQuery = _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            productsQuery = productsQuery.Where(x => x.Name.Contains(search));
        }


        if (query.CategoryId.HasValue)
            productsQuery = productsQuery.Where(x => x.CategoryId == query.CategoryId.Value);

        if (query.IsActive.HasValue)
            productsQuery = productsQuery.Where(x => x.IsActive == query.IsActive.Value);

        productsQuery = query.SortBy switch
        {
            "name" => query.SortDesc
                ? productsQuery.OrderByDescending(x => x.Name)
                : productsQuery.OrderBy(x => x.Name),

            "categoryName" => query.SortDesc
                ? productsQuery.OrderByDescending(x => x.Category.Name)
                : productsQuery.OrderBy(x => x.Category.Name),

            _ => productsQuery.OrderBy(x => x.Name)
        };


        var totalCount = await productsQuery.CountAsync();
        var skip = (query.Page - 1) * query.PageSize;

        var items = await productsQuery
    .Skip(skip)
    .Take(query.PageSize)
    .Select(x => new ProductDto
    {
        Id = x.Id,
        Name = x.Name,
        CategoryId = x.CategoryId,
        CategoryName = x.Category.Name,
        Unit = x.Unit,
        ImagePath = x.ImagePath,
        IsActive = x.IsActive
    })
    .ToListAsync();

        return new PagedResult<ProductDto>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            Items = items
        };

    }

    // =====================================================
    // ACTIVATE / DEACTIVATE
    // =====================================================
    public async Task ActivateAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            throw new NotFoundException("محصول مورد نظر یافت نشد.");

        if (product.IsActive)
            throw new AppException("محصول در حال حاضر فعال است.");

        product.IsActive = true;
        await _context.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            throw new NotFoundException("محصول مورد نظر یافت نشد.");

        if (!product.IsActive)
            throw new AppException("محصول در حال حاضر غیرفعال است.");

        product.IsActive = false;
        await _context.SaveChangesAsync();
    }

    // =====================================================
    // MAPPER
    // =====================================================
}
