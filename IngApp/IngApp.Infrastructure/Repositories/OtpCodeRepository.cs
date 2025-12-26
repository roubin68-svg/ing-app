using IngApp.Application.Common.Interfaces.Authentication;
using IngApp.Domain.Entities.Auth;
using IngApp.Domain.Enums;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Repositories;

public class OtpCodeRepository : IOtpCodeRepository
{
    private readonly AppDbContext _context;

    public OtpCodeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OtpCode?> GetLatestActiveOtpAsync(string phoneNumber, OtpPurpose purpose)
    {
        return await _context.OtpCodes
            .Where(x => x.PhoneNumber == phoneNumber &&
                        x.Purpose == purpose &&
                        !x.IsUsed &&
                        x.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(OtpCode otp)
    {
        await _context.OtpCodes.AddAsync(otp);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
