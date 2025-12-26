using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Products;
using IngApp.Application.Features.Products.DTO;
using IngApp.Domain.Entities.Products;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Products;

public class ProductAttributeTemplateService : IProductAttributeTemplateService
{
    private readonly AppDbContext _db;

    public ProductAttributeTemplateService(AppDbContext db)
    {
        _db = db;
    }

    // =========================
    // GET TEMPLATE BY PRODUCT
    // =========================
    public async Task<List<ProductAttributeTemplateItemDto>> GetByProductAsync(int productId)
    {
        return await _db.ProductAttributeTemplates
            .Include(x => x.AttributeDefinition)
            .Where(x => x.ProductId == productId)
            .Select(x => new ProductAttributeTemplateItemDto
            {
                AttributeDefinitionId = x.AttributeDefinitionId,
                DisplayName = x.AttributeDefinition.DisplayName,
                DataType = (int)x.AttributeDefinition.DataType,
                IsRequired = x.IsRequired
            })
            .ToListAsync();
    }

    // =========================
    // UPSERT TEMPLATE
    // =========================
    public async Task UpsertAsync(CreateProductAttributeTemplateRequest request)
    {
        var errors = new List<string>();

        if (request.ProductId <= 0)
            errors.Add("ProductId نامعتبر است.");

        if (request.Requirements == null)
            errors.Add("لیست Requirements نمی‌تواند خالی باشد.");

        foreach (var item in request.Requirements)
        {
            if (item.AttributeDefinitionId <= 0)
                errors.Add("AttributeDefinitionId نامعتبر است.");
        }

        if (errors.Any())
            throw new ValidationException(errors);

        // -------------------------
        // Load current templates
        // -------------------------
        var existingTemplates = await _db.ProductAttributeTemplates
            .Where(x => x.ProductId == request.ProductId)
            .ToListAsync();

        var incomingAttributeIds = request.Requirements
            .Select(x => x.AttributeDefinitionId)
            .ToHashSet();

        // -------------------------
        // DELETE removed attributes
        // -------------------------
        foreach (var template in existingTemplates)
        {
            if (!incomingAttributeIds.Contains(template.AttributeDefinitionId))
            {
                _db.ProductAttributeTemplates.Remove(template);
            }
        }

        // -------------------------
        // INSERT or UPDATE
        // -------------------------
        foreach (var item in request.Requirements)
        {
            var existing = existingTemplates.FirstOrDefault(x =>
                x.AttributeDefinitionId == item.AttributeDefinitionId);

            if (existing == null)
            {
                _db.ProductAttributeTemplates.Add(new ProductAttributeTemplate
                {
                    ProductId = request.ProductId,
                    AttributeDefinitionId = item.AttributeDefinitionId,
                    IsRequired = item.IsRequired
                });
            }
            else
            {
                existing.IsRequired = item.IsRequired;
            }
        }

        await _db.SaveChangesAsync();
    }
}
