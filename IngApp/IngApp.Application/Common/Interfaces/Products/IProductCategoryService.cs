using IngApp.Application.Common.Models;
using IngApp.Application.Features.Products.DTO;

namespace IngApp.Application.Common.Interfaces.Products;

public interface IProductCategoryService
{
    Task<List<ProductCategoryDto>> GetAllAsync();

    Task<ProductCategoryDto> CreateAsync(CreateProductCategoryRequest request);

    Task<ProductCategoryDto> UpdateAsync(int id, UpdateProductCategoryRequest request);

    Task ActivateAsync(int id);

    Task DeactivateAsync(int id);
}
