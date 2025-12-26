using IngApp.Application.Common.Exceptions;
using IngApp.Application.Features.Kyc.DTO;
using IngApp.Domain.Entities;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Kyc;

public class KycTemplateService : IKycTemplateService
{
    private readonly AppDbContext _db;

    public KycTemplateService(AppDbContext db)
    {
        _db = db;
    }

    // =========================
    // GET TEMPLATE BY SUPPLIER TYPE
    // =========================
    public async Task<List<KycTemplateItemDto>> GetBySupplierTypeAsync(int supplierTypeId)
    {
        return await _db.KycTemplates
            .Include(x => x.KycAttributeDefinition)
            .Where(x => x.SupplierTypeId == supplierTypeId && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new KycTemplateItemDto
            {
                AttributeDefinitionId = x.KycAttributeDefinitionId,
                DisplayName = x.KycAttributeDefinition.DisplayName,
                DataType = (int)x.KycAttributeDefinition.DataType,
                IsRequired = x.IsRequired,
                SortOrder = x.SortOrder
            })
            .ToListAsync();
    }

    // =========================
    // UPSERT TEMPLATE
    // =========================
    public async Task UpsertAsync(CreateKycTemplateRequest request)
    {
        var errors = new List<string>();

        if (request.SupplierTypeId <= 0)
            errors.Add("SupplierTypeId نامعتبر است.");

        if (request.Requirements == null)
            errors.Add("لیست Requirements نمی‌تواند خالی باشد.");

        foreach (var item in request.Requirements)
        {
            if (item.AttributeDefinitionId <= 0)
                errors.Add("AttributeDefinitionId نامعتبر است.");

            if (item.SortOrder <= 0)
                errors.Add("SortOrder باید بزرگتر از صفر باشد.");
        }

        if (errors.Any())
            throw new ValidationException(errors);

        // -------------------------
        // Load current templates
        // -------------------------
        var existingTemplates = await _db.KycTemplates
            .Where(x => x.SupplierTypeId == request.SupplierTypeId)
            .ToListAsync();

        var incomingAttributeIds = request.Requirements
            .Select(x => x.AttributeDefinitionId)
            .ToHashSet();

        // -------------------------
        // SOFT DELETE removed attributes
        // -------------------------
        foreach (var template in existingTemplates)
        {
            if (!incomingAttributeIds.Contains(template.KycAttributeDefinitionId))
            {
                template.IsActive = false;
            }
        }

        // -------------------------
        // INSERT or UPDATE
        // -------------------------
        foreach (var item in request.Requirements)
        {
            var existing = existingTemplates.FirstOrDefault(x =>
                x.KycAttributeDefinitionId == item.AttributeDefinitionId);

            if (existing == null)
            {
                _db.KycTemplates.Add(new KycTemplate
                {
                    SupplierTypeId = request.SupplierTypeId,
                    KycAttributeDefinitionId = item.AttributeDefinitionId,
                    IsRequired = item.IsRequired,
                    SortOrder = item.SortOrder,
                    IsActive = true
                });
            }
            else
            {
                existing.IsRequired = item.IsRequired;
                existing.SortOrder = item.SortOrder;
                existing.IsActive = true;
            }
        }

        await _db.SaveChangesAsync();
    }
}
