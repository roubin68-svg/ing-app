using IngApp.Application.Common.Models;
using IngApp.Application.Features.Offers.DTO;
using IngApp.Application.Features.Offers.Queries;
using IngApp.Application.Features.Offers.Requests;

namespace IngApp.Application.Common.Interfaces.Offers;

public interface IOfferService
{
    Task<int> CreateDraftAsync(Guid supplierUserId, CreateDraftOfferRequest request);

    Task UpdateHeaderAsync(Guid supplierUserId, int offerId, UpdateOfferHeaderRequest request);

    Task SaveDocumentsAsync(Guid supplierUserId, int offerId, SaveOfferDocumentsRequest request);

    Task<OfferDetailDto> GetDetailAsync(Guid supplierUserId, int offerId);

    Task<PagedResult<OfferListItemDto>> GetMyOffersAsync(Guid supplierUserId, MyOffersQuery query);

    Task SubmitAsync(Guid supplierUserId, int offerId);

    Task CancelAsync(Guid supplierUserId, int offerId, string? reason);
    Task<List<PublicOfferListItemDto>> SearchPublicAsync(PublicOfferSearchQuery query);

    Task<OfferDetailDto> GetPublicDetailAsync(int offerId);
    Task<List<AvailableProductCategoryNodeDto>> GetAvailableProductsForOfferAsync(Guid supplierUserId);
    Task ChangeProductAsync(Guid supplierUserId, int offerId, ChangeOfferProductRequest request);
    Task EnsureEditableDraftAsync(Guid supplierUserId, int offerId);
    Task DeleteDocumentFileAsync(Guid supplierUserId, int offerId, string filePath);

}
