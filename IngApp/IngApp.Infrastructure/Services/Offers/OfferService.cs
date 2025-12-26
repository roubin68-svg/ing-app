using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Offers;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Offers.DTO;
using IngApp.Application.Features.Offers.Queries;
using IngApp.Application.Features.Offers.Requests;
using IngApp.Domain.Entities.Offers;
using IngApp.Domain.Enums;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Offers;

public class OfferService : IOfferService
{
    private readonly AppDbContext _db;

    public OfferService(AppDbContext db)
    {
        _db = db;
    }

    // --------------------------------------------------
    // Create Draft Offer
    // --------------------------------------------------
    public async Task<int> CreateDraftAsync(Guid supplierUserId, CreateDraftOfferRequest request)
    {
        if (request.ProductId <= 0)
            throw new ValidationException(new() { "محصول انتخاب‌شده نامعتبر است." });

        // Product exists
        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ProductId);

        if (product == null)
            throw new NotFoundException("محصول موردنظر یافت نشد.");

        // Supplier has access to product category
        var hasAccess = await _db.SupplierCategoryAccesses
            .AnyAsync(x =>
                x.UserId == supplierUserId &&
                x.ProductCategoryId == product.CategoryId &&
                x.IsActive);

        if (!hasAccess)
            throw new ValidationException(new()
            {
                "شما اجازه ثبت آگهی برای این دسته‌بندی محصول را ندارید."
            });

        var now = DateTime.UtcNow;

        var offer = new Offer
        {
            SupplierUserId = supplierUserId,
            ProductId = request.ProductId,
            WizardStep = OfferWizardStep.MainInfo,
            Status = OfferStatus.Draft,

            UnitPrice = 0,
            TotalPrice = 0,
            Quantity = 0,
            Unit = product.Unit,

            HasTax = false,

            CreatedAt = now,
            SearchDateTime = now
        };

        _db.Offers.Add(offer);
        await _db.SaveChangesAsync();

        return offer.Id;
    }

    // --------------------------------------------------
    // Update Offer Header (Draft only)
    // --------------------------------------------------
    public async Task UpdateHeaderAsync(Guid supplierUserId, int offerId, UpdateOfferHeaderRequest request)
    {
        var offer = await _db.Offers
            .FirstOrDefaultAsync(x => x.Id == offerId);

        if (offer == null)
            throw new NotFoundException("آگهی موردنظر یافت نشد.");

        if (offer.SupplierUserId != supplierUserId)
            throw new ValidationException(new() { "دسترسی غیرمجاز به آگهی." });

        if (offer.Status != OfferStatus.Draft)
            throw new ValidationException(new()
            {
                "فقط آگهی‌هایی که در وضعیت پیش‌نویس هستند قابل ویرایش می‌باشند."
            });

        offer.UnitPrice = request.UnitPrice;
        offer.Quantity = request.Quantity;
        offer.TotalPrice = request.UnitPrice * request.Quantity;

        offer.Unit = request.Unit;
        offer.HasTax = request.HasTax;
        offer.TaxAmount = request.TaxAmount;

        offer.ExpireAtBySupplier = request.ExpireAtBySupplier;
        offer.UpdatedAt = DateTime.UtcNow;
        offer.WizardStep = OfferWizardStep.Attributes;

        await _db.SaveChangesAsync();
    }

    // --------------------------------------------------
    // Get Offer Detail (Supplier)
    // --------------------------------------------------
    public async Task<OfferDetailDto> GetDetailAsync(Guid supplierUserId, int offerId)
    {
        var offer = await _db.Offers
            .Include(x => x.Documents.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == offerId);

        if (offer == null)
            throw new NotFoundException("آگهی موردنظر یافت نشد.");

        if (offer.SupplierUserId != supplierUserId)
            throw new ValidationException(new() { "دسترسی غیرمجاز به آگهی." });

        return new OfferDetailDto
        {
            Header = new OfferHeaderDto
            {
                Id = offer.Id,
                ProductId = offer.ProductId,
                UnitPrice = offer.UnitPrice,
                TotalPrice = offer.TotalPrice,
                Quantity = offer.Quantity,
                Unit = offer.Unit,
                HasTax = offer.HasTax,
                TaxAmount = offer.TaxAmount,
                Status = offer.Status,
                CreatedAt = offer.CreatedAt,
                PublishedAt = offer.PublishedAt,
                ExpireAtBySupplier = offer.ExpireAtBySupplier,
                WizardStep = offer.WizardStep,
            },
            Documents = offer.Documents.Select(d => new OfferDocumentDto
            {
                AttributeDefinitionId = d.AttributeDefinitionId,
                Value = d.Value,
                FilePath = d.FilePath
            }).ToList()
        };
    }

    // --------------------------------------------------
    // Get My Offers
    // --------------------------------------------------
    public async Task<PagedResult<OfferListItemDto>> GetMyOffersAsync(Guid supplierUserId, MyOffersQuery query)
    {
        var offersQuery =
            from offer in _db.Offers.AsNoTracking()
            join product in _db.Products on offer.ProductId equals product.Id
            join category in _db.ProductCategories on product.CategoryId equals category.Id
            where offer.SupplierUserId == supplierUserId
            select new { offer, product, category };

        // -----------------------
        // Filters
        // -----------------------
        if (query.Status.HasValue)
            offersQuery = offersQuery.Where(x => x.offer.Status == query.Status);

        if (query.ProductCategoryId.HasValue)
            offersQuery = offersQuery.Where(x => x.category.Id == query.ProductCategoryId);

        if (!string.IsNullOrWhiteSpace(query.ProductName))
            offersQuery = offersQuery.Where(x =>
                x.product.Name.Contains(query.ProductName));

        // -----------------------
        // Sorting
        // -----------------------
        offersQuery = query.SortBy switch
        {
            "productName" => query.SortDirection == "asc"
                ? offersQuery.OrderBy(x => x.product.Name)
                : offersQuery.OrderByDescending(x => x.product.Name),

            "createdAt" => query.SortDirection == "asc"
                ? offersQuery.OrderBy(x => x.offer.CreatedAt)
                : offersQuery.OrderByDescending(x => x.offer.CreatedAt),

            _ => offersQuery.OrderByDescending(x => x.offer.CreatedAt)
        };

        // -----------------------
        // Paging
        // -----------------------
        var totalCount = await offersQuery.CountAsync();

        var items = await offersQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new OfferListItemDto
            {
                Id = x.offer.Id,
                ProductId = x.product.Id,
                ProductName = x.product.Name,
                ProductCategoryId = x.category.Id,
                ProductCategoryName = x.category.Name,
                Quantity = x.offer.Quantity,
                Unit = x.offer.Unit,
                TotalPrice = x.offer.TotalPrice,
                Status = x.offer.Status,
                CreatedAt = x.offer.CreatedAt,
                PublishedAt = x.offer.PublishedAt
            })
            .ToListAsync();

        return new PagedResult<OfferListItemDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }


    // --------------------------------------------------
    // NOT IMPLEMENTED YET (Next phases)
    // --------------------------------------------------
    public async Task SaveDocumentsAsync(Guid supplierUserId, int offerId, SaveOfferDocumentsRequest request)
    {
        var offer = await _db.Offers
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == offerId);

        if (offer == null)
            throw new NotFoundException("آگهی موردنظر یافت نشد.");

        if (offer.SupplierUserId != supplierUserId)
            throw new ValidationException(new() { "دسترسی غیرمجاز به آگهی." });

        if (offer.Status != OfferStatus.Draft)
            throw new ValidationException(new()
        {
            "فقط آگهی‌هایی که در وضعیت پیش‌نویس هستند قابل ویرایش می‌باشند."
        });

        // -----------------------------
        // Load Product Template
        // -----------------------------
        var templateItems = await _db.ProductAttributeTemplates
            .AsNoTracking()
            .Where(x => x.ProductId == offer.ProductId)
            .ToListAsync();

        if (!templateItems.Any())
            throw new ValidationException(new()
        {
            "برای این محصول قالب ویژگی تعریف نشده است."
        });

        var templateAttributeIds = templateItems
            .Select(x => x.AttributeDefinitionId)
            .ToHashSet();

        // -----------------------------
        // Validate incoming attributes
        // -----------------------------
        foreach (var item in request.Items)
        {
            if (!templateAttributeIds.Contains(item.AttributeDefinitionId))
                throw new ValidationException(new()
            {
                "یکی از ویژگی‌های ارسال‌شده معتبر نیست."
            });
        }

        // -----------------------------
        // Clear existing documents
        // -----------------------------
        var now = DateTime.UtcNow;

        foreach (var doc in offer.Documents.Where(d => !d.IsDeleted))
        {
            doc.IsDeleted = true;
            doc.DeletedAt = now;
        }


        // -----------------------------
        // Save new documents
        // -----------------------------
        foreach (var item in request.Items)
        {
            _db.OfferDocuments.Add(new OfferDocument
            {
                OfferId = offer.Id,
                AttributeDefinitionId = item.AttributeDefinitionId,
                Value = item.Value,
                FilePath = item.FilePath,
                IsDeleted = false,
                DeletedAt = null
            });
        }
        offer.WizardStep = OfferWizardStep.Review;


        await _db.SaveChangesAsync();
    }

    public async Task SubmitAsync(Guid supplierUserId, int offerId)
    {
        var offer = await _db.Offers
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == offerId);

        if (offer == null)
            throw new NotFoundException("آگهی موردنظر یافت نشد.");

        if (offer.SupplierUserId != supplierUserId)
            throw new ValidationException(new() { "دسترسی غیرمجاز به آگهی." });

        if (offer.Status != OfferStatus.Draft)
            throw new ValidationException(new()
        {
            "فقط آگهی‌های پیش‌نویس قابل ارسال می‌باشند."
        });

        // -----------------------------
        // Load Template
        // -----------------------------
        var templateItems = await _db.ProductAttributeTemplates
            .AsNoTracking()
            .Where(x => x.ProductId == offer.ProductId)
            .ToListAsync();

        if (!templateItems.Any())
            throw new ValidationException(new()
        {
            "برای این محصول قالب ویژگی تعریف نشده است."
        });

        // -----------------------------
        // Validate Required Attributes
        // -----------------------------
        foreach (var template in templateItems)
        {
            if (!template.IsRequired)
                continue;

            var doc = offer.Documents
                .FirstOrDefault(x => x.AttributeDefinitionId == template.AttributeDefinitionId);

            if (doc == null)
                throw new ValidationException(new()
            {
                "تمامی ویژگی‌های الزامی باید تکمیل شوند."
            });

            var attr = await _db.ProductAttributeDefinitions
                .AsNoTracking()
                .FirstAsync(x => x.Id == template.AttributeDefinitionId);

            if (attr.DataType == ProductAttributeDataType.File)
            {
                if (string.IsNullOrWhiteSpace(doc.FilePath))
                    throw new ValidationException(new()
                {
                    "فایل مربوط به یکی از ویژگی‌های الزامی بارگذاری نشده است."
                });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(doc.Value))
                    throw new ValidationException(new()
                {
                    "مقدار یکی از ویژگی‌های الزامی تکمیل نشده است."
                });
            }
        }

        // -----------------------------
        // Validate Header
        // -----------------------------
        if (offer.Quantity <= 0)
            throw new ValidationException(new() { "مقدار آگهی معتبر نیست." });

        if (offer.TotalPrice <= 0)
            throw new ValidationException(new() { "قیمت کل آگهی معتبر نیست." });

        // -----------------------------
        // Publish
        // -----------------------------
        var now = DateTime.UtcNow;

        offer.Status = OfferStatus.Published; // یا Pending (آینده)
        offer.PublishedAt = now;
        offer.SearchDateTime = now;
        offer.UpdatedAt = now;
        offer.WizardStep = OfferWizardStep.Review;

        await _db.SaveChangesAsync();
    }

    public async Task CancelAsync(Guid supplierUserId, int offerId, string? reason)
    {
        var offer = await _db.Offers
            .FirstOrDefaultAsync(x => x.Id == offerId);

        if (offer == null)
            throw new NotFoundException("آگهی موردنظر یافت نشد.");

        if (offer.SupplierUserId != supplierUserId)
            throw new ValidationException(new() { "دسترسی غیرمجاز به آگهی." });

        if (offer.Status == OfferStatus.Cancel)
            return;

        offer.Status = OfferStatus.Cancel;
        offer.CancelReason = reason;
        offer.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<List<PublicOfferListItemDto>> SearchPublicAsync(PublicOfferSearchQuery query)
    {
        var offers =
    from offer in _db.Offers.AsNoTracking()
    join product in _db.Products on offer.ProductId equals product.Id
    where offer.Status == OfferStatus.Published
    select new { offer, product };

        if (query.CategoryId.HasValue)
        {
            offers = offers.Where(x => x.product.CategoryId == query.CategoryId.Value);
        }

        if (query.ProductId.HasValue)
            offers = offers.Where(x => x.offer.ProductId == query.ProductId);

        if (query.MinPrice.HasValue)
            offers = offers.Where(x => x.offer.TotalPrice >= query.MinPrice);

        if (query.MaxPrice.HasValue)
            offers = offers.Where(x => x.offer.TotalPrice <= query.MaxPrice);


        return await offers
    .OrderByDescending(x => x.offer.SearchDateTime)
    .Skip((query.Page - 1) * query.PageSize)
    .Take(query.PageSize)
    .Select(x => new PublicOfferListItemDto
    {
        Id = x.offer.Id,
        ProductId = x.offer.ProductId,
        TotalPrice = x.offer.TotalPrice,
        Quantity = x.offer.Quantity,
        Unit = x.offer.Unit,
        PublishedAt = x.offer.PublishedAt!.Value
    })
    .ToListAsync();

    }

    public async Task<OfferDetailDto> GetPublicDetailAsync(int offerId)
    {
        var offer = await _db.Offers
            .Include(x => x.Documents.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(x =>
                x.Id == offerId &&
                x.Status == OfferStatus.Published);

        if (offer == null)
            throw new NotFoundException("آگهی موردنظر یافت نشد.");

        return new OfferDetailDto
        {
            Header = new OfferHeaderDto
            {
                Id = offer.Id,
                ProductId = offer.ProductId,
                UnitPrice = offer.UnitPrice,
                TotalPrice = offer.TotalPrice,
                Quantity = offer.Quantity,
                Unit = offer.Unit,
                HasTax = offer.HasTax,
                TaxAmount = offer.TaxAmount,
                Status = offer.Status,
                CreatedAt = offer.CreatedAt,
                PublishedAt = offer.PublishedAt,
                ExpireAtBySupplier = offer.ExpireAtBySupplier,
                WizardStep = offer.WizardStep
            },
            Documents = offer.Documents.Select(d => new OfferDocumentDto
            {
                AttributeDefinitionId = d.AttributeDefinitionId,
                Value = d.Value,
                FilePath = d.FilePath
            }).ToList()
        };
    }

    public async Task<List<AvailableProductCategoryNodeDto>> GetAvailableProductsForOfferAsync(Guid supplierUserId)
    {
        // 1. Categoryهایی که Supplier بهش دسترسی دارد
        var allowedCategoryIds = await _db.SupplierCategoryAccesses
            .Where(x => x.UserId == supplierUserId && x.IsActive)
            .Select(x => x.ProductCategoryId)
            .ToListAsync();

        if (!allowedCategoryIds.Any())
            return new();

        // 2. گرفتن Category + Parent
        var categories = await _db.ProductCategories
            .AsNoTracking()
            .Where(c => allowedCategoryIds.Contains(c.Id))
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.ParentId
            })
            .ToListAsync();

        // 3. گرفتن Products
        var products = await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && allowedCategoryIds.Contains(p.CategoryId))
            .Select(p => new AvailableProductForOfferDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name
            })
            .ToListAsync();

        // 4. ساخت map از category
        var categoryMap = categories.ToDictionary(
            c => c.Id,
            c => new AvailableProductCategoryNodeDto
            {
                Id = c.Id,
                Name = c.Name
            });

        // 5. اتصال child → parent
        foreach (var c in categories)
        {
            if (c.ParentId.HasValue && categoryMap.ContainsKey(c.ParentId.Value))
            {
                categoryMap[c.ParentId.Value]
                    .Children
                    .Add(categoryMap[c.Id]);
            }
        }

        // 6. الصاق products به category
        foreach (var p in products)
        {
            if (categoryMap.TryGetValue(p.CategoryId, out var node))
            {
                node.Products.Add(p);
            }
        }

        // 7. فقط root categoryها
        var rootNodes = categories
            .Where(c => !c.ParentId.HasValue)
            .Select(c => categoryMap[c.Id])
            .Where(n => n.Children.Any() || n.Products.Any())
            .ToList();

        return rootNodes;
    }

    public async Task EnsureEditableDraftAsync(Guid supplierUserId, int offerId)
    {
        var offer = await _db.Offers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == offerId);

        if (offer == null)
            throw new NotFoundException("آگهی موردنظر یافت نشد.");

        if (offer.SupplierUserId != supplierUserId)
            throw new ValidationException(new() { "دسترسی غیرمجاز به آگهی." });

        if (offer.Status != OfferStatus.Draft)
            throw new ValidationException(new()
        {
            "فقط آگهی‌هایی که در وضعیت پیش‌نویس هستند قابل ویرایش می‌باشند."
        });
    }

    public async Task ChangeProductAsync(Guid supplierUserId, int offerId, ChangeOfferProductRequest request)
    {
        if (request.ProductId <= 0)
            throw new ValidationException(new() { "محصول انتخاب‌شده نامعتبر است." });

        var offer = await _db.Offers
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == offerId);

        if (offer == null)
            throw new NotFoundException("آگهی موردنظر یافت نشد.");

        if (offer.SupplierUserId != supplierUserId)
            throw new ValidationException(new() { "دسترسی غیرمجاز به آگهی." });

        if (offer.Status != OfferStatus.Draft)
            throw new ValidationException(new()
        {
            "فقط آگهی‌هایی که در وضعیت پیش‌نویس هستند قابل ویرایش می‌باشند."
        });

        if (offer.ProductId == request.ProductId)
            return;

        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ProductId);

        if (product == null)
            throw new NotFoundException("محصول موردنظر یافت نشد.");

        var hasAccess = await _db.SupplierCategoryAccesses
            .AnyAsync(x =>
                x.UserId == supplierUserId &&
                x.ProductCategoryId == product.CategoryId &&
                x.IsActive);

        if (!hasAccess)
            throw new ValidationException(new()
        {
            "شما اجازه ثبت آگهی برای این دسته‌بندی محصول را ندارید."
        });

        var now = DateTime.UtcNow;

        // 1) تغییر محصول
        offer.ProductId = product.Id;
        offer.Unit = product.Unit;

        // 2) پاکسازی کامل اطلاعات (header)
        offer.UnitPrice = 0;
        offer.Quantity = 0;
        offer.TotalPrice = 0;

        offer.HasTax = false;
        offer.TaxAmount = null;

        offer.ExpireAtBySupplier = null;
        offer.ExpireAtBySystem = null;

        // 3) پاکسازی documents (soft delete)
        foreach (var doc in offer.Documents.Where(d => !d.IsDeleted))
        {
            doc.IsDeleted = true;
            doc.DeletedAt = now;
        }

        // 4) مرحله
        offer.WizardStep = OfferWizardStep.MainInfo;
        offer.UpdatedAt = now;

        await _db.SaveChangesAsync();
    }

}
