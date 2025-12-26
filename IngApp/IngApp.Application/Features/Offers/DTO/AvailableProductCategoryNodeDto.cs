namespace IngApp.Application.Features.Offers.DTO;

public class AvailableProductCategoryNodeDto
{
    public int Id { get; set; }
    public string Name { get; set; }

    public List<AvailableProductCategoryNodeDto> Children { get; set; } = new();

    public List<AvailableProductForOfferDto> Products { get; set; } = new();
}
