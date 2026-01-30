using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Common.Models;
using IngApp.Application.Common.Security;
using IngApp.Application.Features.Financial.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/wallet-management")]
[Authorize(Policy = Permissions.Financial.Manage)]
public class WalletManagementController : ControllerBase
{
    private readonly IWalletManagementService _service;
    private readonly ICommissionService _commissionService;

    public WalletManagementController(
        IWalletManagementService service,
        ICommissionService commissionService)
    {
        _service = service;
        _commissionService = commissionService;
    }

    // GET: لیست کاربران به همراه خلاصه کیف پول
    [HttpGet("users")]
    public async Task<IActionResult> GetWalletUsers([FromQuery] WalletUserListQueryDto query)
    {
        var result = await _service.GetWalletUsersAsync(query);
        return Ok(ApiResult.Ok(result));
    }

    // GET: گزارش دفتر کل تراکنش‌های مالی (همه کاربران)
    [HttpGet("transactions")]
    public async Task<IActionResult> GetAllTransactions([FromQuery] WalletTransactionListQueryDto query)
    {
        var report = await _service.GetAllTransactionsAsync(query);
        return Ok(ApiResult.Ok(report));
    }

    // GET: گزارش پورسانت‌ها
    [HttpGet("commissions-report")]
    public async Task<IActionResult> GetCommissionsReport([FromQuery] CommissionReportQueryDto query)
    {
        var report = await _commissionService.GetCommissionReportAsync(query);
        return Ok(ApiResult.Ok(report));
    }

    // GET: گزارش درآمد/هزینه
    [HttpGet("income-expense-report")]
    public async Task<IActionResult> GetIncomeExpenseReport([FromQuery] IncomeExpenseReportQueryDto query)
    {
        var report = await _service.GetIncomeExpenseReportAsync(query);
        return Ok(ApiResult.Ok(report));
    }

    // GET: داشبورد مالی
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetFinancialDashboard()
    {
        var dashboard = await _service.GetFinancialDashboardAsync();
        return Ok(ApiResult.Ok(dashboard));
    }

    // GET: دریافت موجودی کیف پول یک کاربر
    [HttpGet("users/{userId:guid}/balance")]
    public async Task<IActionResult> GetUserBalance(Guid userId)
    {
        var balance = await _service.GetUserBalanceAsync(userId);
        return Ok(ApiResult.Ok(balance));
    }

    // GET: دریافت لیست تراکنش‌های یک کاربر
    [HttpGet("users/{userId:guid}/transactions")]
    public async Task<IActionResult> GetUserTransactions(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var transactions = await _service.GetUserTransactionsAsync(userId, page, pageSize);
        return Ok(ApiResult.Ok(transactions));
    }

    // POST: واریز دستی به کیف پول کاربر
    [HttpPost("users/{userId:guid}/deposit")]
    public async Task<IActionResult> ManualDeposit(
        Guid userId,
        [FromBody] ManualWalletTransactionDto dto)
    {
        var result = await _service.ManualDepositAsync(
            userId,
            dto.AmountRial,
            dto.Description ?? string.Empty,
            dto.IsBankSettlement);
        return Ok(ApiResult.Ok(result));
    }

    // POST: برداشت دستی از کیف پول کاربر
    [HttpPost("users/{userId:guid}/withdrawal")]
    public async Task<IActionResult> ManualWithdrawal(
        Guid userId,
        [FromBody] ManualWalletTransactionDto dto)
    {
        var result = await _service.ManualWithdrawalAsync(
            userId,
            dto.AmountRial,
            dto.Description ?? string.Empty,
            dto.IsBankSettlement);
        return Ok(ApiResult.Ok(result));
    }
}

