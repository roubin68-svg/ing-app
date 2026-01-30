namespace IngApp.Domain.Entities.Settings;

/// <summary>
/// تنظیمات سیستم
/// </summary>
public class SystemSetting
{
    public int Id { get; set; }
    
    /// <summary>
    /// کلید تنظیمات (مثلاً SubscriptionCancellationServiceFeePercentage)
    /// </summary>
    public string Key { get; set; } = null!;
    
    /// <summary>
    /// مقدار تنظیمات (می‌تواند عدد، رشته، JSON و غیره باشد)
    /// </summary>
    public string Value { get; set; } = null!;
    
    /// <summary>
    /// عنوان نمایشی
    /// </summary>
    public string DisplayName { get; set; } = null!;
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// نوع داده (Number, String, Boolean, Json)
    /// </summary>
    public string DataType { get; set; } = "String";
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}



