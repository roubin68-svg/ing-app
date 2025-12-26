using IngApp.Application.Common.Models;
using IngApp.Application.Features.Products.Attributes;
using IngApp.Application.Features.Products.DTO;

namespace IngApp.Application.Common.Interfaces.Products;

public interface IProductAttributeDefinitionService
{
    Task<PagedResult<ProductAttributeDefinitionDto>> GetPagedAsync(ProductAttributeDefinitionListQuery query);

    Task<List<ProductAttributeDefinitionDto>> GetAllAsync();
    Task<List<ProductAttributeDefinitionDto>> GetActiveAsync();

    Task<ProductAttributeDefinitionDto?> GetByIdAsync(int id);

    Task<ProductAttributeDefinitionDto> CreateAsync(CreateProductAttributeDefinitionRequest request);
    Task<ProductAttributeDefinitionDto> UpdateAsync(int id, UpdateProductAttributeDefinitionRequest request);

    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);
}
