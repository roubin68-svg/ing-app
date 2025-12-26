using IngApp.Application.Common.Models;
using IngApp.Application.Features.Products.DTO;

namespace IngApp.Application.Common.Interfaces.Products;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetPagedAsync(ProductListQuery query);

    Task<ProductDto> GetByIdAsync(int id);

    Task<ProductDto> CreateAsync(CreateProductRequest request);

    Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request);

    Task ActivateAsync(int id);

    Task DeactivateAsync(int id);
}
