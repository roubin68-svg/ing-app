using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "uid");
        if (claim == null)
            throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        return Guid.Parse(claim.Value);
    }

    // GET: دریافت موجودی کیف پول
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var userId = GetCurrentUserId();
        var balance = await _walletService.GetBalanceAsync(userId);
        return Ok(ApiResult.Ok(balance));
    }

    // GET: دریافت لیست تراکنش‌های کیف پول
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var transactions = await _walletService.GetTransactionsAsync(userId, page, pageSize);
        return Ok(ApiResult.Ok(transactions));
    }
}











