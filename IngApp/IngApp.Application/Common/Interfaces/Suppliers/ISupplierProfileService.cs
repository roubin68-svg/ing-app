using IngApp.Application.Common.Models;
using IngApp.Application.Features.Suppliers.DTO;
using IngApp.Domain.Enums;

namespace IngApp.Application.Common.Interfaces.Suppliers
{
    public interface ISupplierProfileService
    {
        Task<PagedResult<SupplierProfileDto>> GetPagedAsync(SupplierListQueryDto filter);

        Task<SupplierProfileDto?> GetByUserIdAsync(Guid userId);

        Task<SupplierProfileDto> UpsertForUserAsync(Guid userId, UpsertSupplierProfileRequest request);

        Task<List<SupplierTypeDto>> GetActiveSupplierTypesAsync();
        Task UpdateVerificationStatusAsync(Guid supplierId, VerificationStatus newStatus, string? note, string adminUserId);

        Task<List<SupplierVerificationHistoryDto>> GetVerificationHistoryAsync(Guid supplierId);
        Task AddActivityAsync(Guid supplierId, string actionType, string? metadataJson, string? userId, string? adminUserId);
        Task<List<SupplierActivityLogDto>> GetActivityLogsAsync(Guid supplierId);
        Task<SupplierDetailDto?> GetSupplierDetailAsync(Guid supplierId);
        Task<SupplierProfileDto?> GetMyAsync(Guid userId);

        Task<int> GetPendingCountAsync();
    }
}
