using IngApp.Application.Common.Models;
using IngApp.Application.Features.Kyc.DTO;

namespace IngApp.Application.Common.Interfaces.Kyc
{
    public interface IKycService
    {
        // برای پنل ادمین – لیست مدارک با Paging
        Task<PagedResult<UserDocumentDto>> GetPagedAsync(KycListQueryDto filter);

        // برای خود کاربر (Buyer/Supplier)
        Task<List<KycRequirementDto>> GetRequirementsForUserAsync(Guid userId);

        Task SubmitDocumentsAsync(Guid userId, List<SubmitKycDocumentItemDto> items);

        Task<List<UserDocumentDto>> GetUserDocumentsAsync(Guid userId);

        // برای Admin – جزئیات یک مدرک
        Task<UserDocumentDto?> GetDocumentByIdAsync(Guid documentId);

        // بررسی / تایید / رد مدرک
        Task ReviewDocumentAsync(Guid documentId, ReviewKycDocumentRequest request);
    }
}
