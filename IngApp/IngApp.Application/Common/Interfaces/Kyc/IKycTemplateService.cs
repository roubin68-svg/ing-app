using IngApp.Application.Features.Kyc.DTO;

public interface IKycTemplateService
{
    Task<List<KycTemplateItemDto>> GetBySupplierTypeAsync(int supplierTypeId);

    Task UpsertAsync(CreateKycTemplateRequest request);
}
