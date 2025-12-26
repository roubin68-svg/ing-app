using IngApp.Application.Features.Products.DTO;

namespace IngApp.Application.Common.Interfaces.Products;

public interface IProductAttributeTemplateService
{
    Task<List<ProductAttributeTemplateItemDto>> GetByProductAsync(int productId);

    Task UpsertAsync(CreateProductAttributeTemplateRequest request);
}