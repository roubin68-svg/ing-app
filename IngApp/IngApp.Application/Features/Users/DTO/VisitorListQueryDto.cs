namespace IngApp.Application.Features.Users.DTO;

public class VisitorListQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; } // جستجو در PhoneNumber, DisplayName, ReferralCode, BusinessName
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } // createdat, referralcode, buyercount, totalcommission
    public bool SortDesc { get; set; } = true;
}











