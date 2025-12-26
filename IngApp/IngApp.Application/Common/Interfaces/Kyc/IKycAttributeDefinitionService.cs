using IngApp.Application.Common.Models;
using IngApp.Application.Features.Kyc.DTO;

namespace IngApp.Application.Common.Interfaces.Kyc
{
    public interface IKycAttributeDefinitionService
    {
        Task<PagedResult<KycAttributeDefinitionDto>> GetPagedAsync(KycAttributeDefinitionListQueryDto filter);

        Task<List<KycAttributeDefinitionDto>> GetAllAsync();
        Task<List<KycAttributeDefinitionDto>> GetActiveAsync();

        Task<KycAttributeDefinitionDto?> GetByIdAsync(int id);

        Task<KycAttributeDefinitionDto> CreateAsync(CreateKycAttributeDefinitionRequest request);
        Task<KycAttributeDefinitionDto> UpdateAsync(int id, UpdateKycAttributeDefinitionRequest request);

        Task ActivateAsync(int id);
        Task DeactivateAsync(int id);
    }
}
