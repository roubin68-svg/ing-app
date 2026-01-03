using IngApp.Application.Common.Interfaces.Offers;
using IngApp.Domain.Entities.Offers;
using IngApp.Domain.Enums;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Offers;

public class OfferClickService : IOfferClickService
{
    private readonly AppDbContext _db;

    public OfferClickService(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogClickAsync(int offerId, OfferClickType clickType, Guid? userId = null, string? ipAddress = null, string? userAgent = null)
    {
        var log = new OfferClickLog
        {
            OfferId = offerId,
            ClickType = clickType,
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ClickedAt = DateTime.UtcNow
        };

        _db.OfferClickLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetClickCountAsync(int offerId, OfferClickType clickType)
    {
        return await _db.OfferClickLogs
            .AsNoTracking()
            .CountAsync(x => x.OfferId == offerId && x.ClickType == clickType);
    }

    public async Task<OfferClickStatsDto> GetClickStatsAsync(int offerId)
    {
        var stats = await _db.OfferClickLogs
            .AsNoTracking()
            .Where(x => x.OfferId == offerId)
            .GroupBy(x => x.ClickType)
            .Select(g => new { ClickType = g.Key, Count = g.Count() })
            .ToListAsync();

        return new OfferClickStatsDto
        {
            OfferId = offerId,
            ViewCount = stats.FirstOrDefault(x => x.ClickType == OfferClickType.View)?.Count ?? 0,
            ContactClickCount = stats.FirstOrDefault(x => x.ClickType == OfferClickType.ContactClick)?.Count ?? 0
        };
    }
}

