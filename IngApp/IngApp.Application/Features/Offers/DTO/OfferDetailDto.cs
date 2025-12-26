
namespace IngApp.Application.Features.Offers.DTO;

public class OfferDetailDto
{
    public OfferHeaderDto Header { get; set; } = null!;
    public List<OfferDocumentDto> Documents { get; set; } = [];
}
