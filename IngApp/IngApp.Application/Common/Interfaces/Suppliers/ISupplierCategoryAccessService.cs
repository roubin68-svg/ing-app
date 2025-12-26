using IngApp.Application.Features.Suppliers.DTO;

namespace IngApp.Application.Common.Interfaces.Suppliers;

public interface ISupplierCategoryAccessService
{
    Task<List<SupplierCategoryAccessDto>> GetByUserIdAsync(Guid userId);

    Task SyncAsync(Guid userId, List<int> productCategoryIds);
}
