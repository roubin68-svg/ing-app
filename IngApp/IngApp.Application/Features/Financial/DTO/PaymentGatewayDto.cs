namespace IngApp.Application.Features.Financial.DTO;

public class PaymentGatewayDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}




















