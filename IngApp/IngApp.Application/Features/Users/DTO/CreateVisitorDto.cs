namespace IngApp.Application.Features.Users.DTO;

public class CreateVisitorDto
{
    /// <summary>
    /// شناسه User که می‌خواهیم برایش VisitorProfile ایجاد کنیم
    /// </summary>
    public Guid UserId { get; set; }
    
    public string? BusinessName { get; set; }
    public string? ContactMobile { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}











