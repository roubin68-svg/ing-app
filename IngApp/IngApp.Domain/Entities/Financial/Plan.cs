using IngApp.Domain.Entities.Users;

namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// پلن/پکیج اشتراک
/// </summary>
public class Plan
{
    public int Id { get; set; }
    public string Code { get; set; } = null!; // Plan1Month, Plan3Month, Plan6Month, Plan12Month
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    
    /// <summary>
    /// مدت زمان اشتراک به ماه (1, 3, 6, 12)
    /// </summary>
    public int DurationMonths { get; set; }
    
    /// <summary>
    /// قیمت به ریال
    /// </summary>
    public long PriceRial { get; set; }
    
    /// <summary>
    /// ویژگی: نمایش نامحدود اطلاعات تماس
    /// </summary>
    public bool UnlimitedContactViews { get; set; }
    
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}












