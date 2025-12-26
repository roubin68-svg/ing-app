using IngApp.Application.Common.Models;
using IngApp.Application.Features.Suppliers.DTO;

namespace IngApp.Application.Common.Interfaces.Suppliers
{
    public interface ISupplierTypeService
    {
        Task<PagedResult<SupplierTypeDto>> GetPagedAsync(SupplierTypeListQueryDto filter);

        Task<List<SupplierTypeDto>> GetAllAsync();
        Task<SupplierTypeDto?> GetByIdAsync(int id);

        Task<SupplierTypeDto> CreateAsync(CreateSupplierTypeRequest request);
        Task<SupplierTypeDto> UpdateAsync(int id, UpdateSupplierTypeRequest request);

        Task ActivateAsync(int id);
        Task DeactivateAsync(int id);
    }
}
