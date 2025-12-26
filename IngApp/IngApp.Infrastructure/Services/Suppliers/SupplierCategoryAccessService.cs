using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Suppliers;
using IngApp.Application.Features.Suppliers.DTO;
using IngApp.Domain.Entities.Products;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Suppliers;

public class SupplierCategoryAccessService : ISupplierCategoryAccessService
{
    private readonly AppDbContext _db;

    public SupplierCategoryAccessService(AppDbContext db)
    {
        _db = db;
    }

    // -----------------------------------------
    // GET: دسترسی‌های Supplier
    // -----------------------------------------
    public async Task<List<SupplierCategoryAccessDto>> GetByUserIdAsync(Guid userId)
    {
        return await _db.SupplierCategoryAccesses
            .AsNoTracking()
            .Include(x => x.ProductCategory)
            .Where(x => x.UserId == userId)
            .Select(x => new SupplierCategoryAccessDto
            {
                ProductCategoryId = x.ProductCategoryId,
                ProductCategoryTitle = x.ProductCategory.Name,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    // -----------------------------------------
    // SYNC: تنظیم دسترسی‌ها
    // -----------------------------------------
    public async Task SyncAsync(Guid userId, List<int> productCategoryIds)
    {
        if (userId == Guid.Empty)
            throw new ValidationException(new() { "شناسه کاربر نامعتبر است." });

        if (productCategoryIds == null)
            throw new ValidationException(new() { "لیست دسته‌بندی‌ها ارسال نشده است." });

        // -----------------------------
        // Load existing accesses
        // -----------------------------
        var existingAccesses = await _db.SupplierCategoryAccesses
            .Where(x => x.UserId == userId)
            .ToListAsync();

        var incomingSet = productCategoryIds.Distinct().ToHashSet();

        // -----------------------------
        // Disable removed categories
        // -----------------------------
        foreach (var access in existingAccesses)
        {
            if (!incomingSet.Contains(access.ProductCategoryId))
            {
                if (access.IsActive)
                    access.IsActive = false;
            }
        }

        // -----------------------------
        // Add or Enable categories
        // -----------------------------
        foreach (var categoryId in incomingSet)
        {
            var existing = existingAccesses
                .FirstOrDefault(x => x.ProductCategoryId == categoryId);

            if (existing == null)
            {
                _db.SupplierCategoryAccesses.Add(new SupplierCategoryAccess
                {
                    UserId = userId,
                    ProductCategoryId = categoryId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                if (!existing.IsActive)
                    existing.IsActive = true;
            }
        }

        await _db.SaveChangesAsync();
    }
}
