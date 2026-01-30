using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Financial.DTO;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Financial;

public class WalletManagementService : IWalletManagementService
{
    private readonly AppDbContext _db;
    private readonly IWalletService _walletService;

    public WalletManagementService(AppDbContext db, IWalletService walletService)
    {
        _db = db;
        _walletService = walletService;
    }

        public async Task<PagedResult<WalletUserSummaryDto>> GetWalletUsersAsync(WalletUserListQueryDto query)
        {
            var usersQuery =
                from u in _db.Users.AsNoTracking()
                join ut in _db.UserTypes.AsNoTracking() on u.UserTypeId equals ut.Id into utJoin
                from ut in utJoin.DefaultIfEmpty()
                join w in _db.Wallets.AsNoTracking().Where(w => w.WalletTypeId == 1) on u.Id equals w.UserId into wJoin
                from w in wJoin.DefaultIfEmpty()
                select new
                {
                    u.Id,
                    u.PhoneNumber,
                    u.DisplayName,
                    u.UserTypeId,
                    UserTypeTitle = ut != null ? ut.Title : null,
                    BalanceRial = w != null ? w.BalanceRial : 0
                };

            if (!string.IsNullOrWhiteSpace(query.PhoneNumber))
            {
                usersQuery = usersQuery.Where(x => x.PhoneNumber.Contains(query.PhoneNumber));
            }

            if (!string.IsNullOrWhiteSpace(query.DisplayName))
            {
                usersQuery = usersQuery.Where(x => x.DisplayName != null && x.DisplayName.Contains(query.DisplayName));
            }

            if (query.UserTypeId.HasValue)
            {
                usersQuery = usersQuery.Where(x => x.UserTypeId == query.UserTypeId.Value);
            }

            if (query.MinBalanceRial.HasValue)
            {
                usersQuery = usersQuery.Where(x => x.BalanceRial >= query.MinBalanceRial.Value);
            }

            if (query.MaxBalanceRial.HasValue)
            {
                usersQuery = usersQuery.Where(x => x.BalanceRial <= query.MaxBalanceRial.Value);
            }

            // فیلتر بر اساس داشتن تراکنش (گردش حساب)
            if (query.HasTransactions == true)
            {
                var usersWithTransactions = _db.WalletTransactions
                    .AsNoTracking()
                    .Where(t => t.Status.Code == "Committed")
                    .Select(t => t.Wallet.UserId)
                    .Distinct();

                usersQuery = usersQuery.Where(x => usersWithTransactions.Contains(x.Id));
            }

            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

            var totalCount = await usersQuery.CountAsync();

            var items = await usersQuery
                .OrderBy(x => x.PhoneNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WalletUserSummaryDto
                {
                    UserId = x.Id,
                    PhoneNumber = x.PhoneNumber,
                    DisplayName = x.DisplayName,
                    UserTypeId = x.UserTypeId,
                    UserTypeTitle = x.UserTypeTitle,
                    BalanceRial = x.BalanceRial
                })
                .ToListAsync();

            return new PagedResult<WalletUserSummaryDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

    public async Task<WalletBalanceDto?> GetUserBalanceAsync(Guid userId)
    {
        return await _walletService.GetBalanceAsync(userId);
    }

    public async Task<PagedResult<WalletTransactionDto>> GetUserTransactionsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20)
    {
        return await _walletService.GetTransactionsAsync(userId, page, pageSize);
    }

    public async Task<WalletTransactionResultDto> ManualDepositAsync(
        Guid userId,
        long amountRial,
        string description,
        bool isBankSettlement = false)
    {
        if (amountRial <= 0)
            throw new ValidationException(new() { "مبلغ واریز باید بیشتر از صفر باشد." });

        // بررسی وجود کاربر
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new NotFoundException("کاربر یافت نشد.");

        // دریافت OperationType و ReferenceType
        var operationType = await _db.FinancialOperationTypes
            .FirstOrDefaultAsync(ot => ot.Code == "ManualDeposit");

        if (operationType == null)
            throw new AppException("نوع عملیات 'ManualDeposit' یافت نشد. لطفاً با مدیر سیستم تماس بگیرید.");

        var referenceType = await _db.FinancialReferenceTypes
            .FirstOrDefaultAsync(rt => rt.Code == "AdminAction");

        if (referenceType == null)
            throw new AppException("نوع مرجع 'AdminAction' یافت نشد. لطفاً با مدیر سیستم تماس بگیرید.");

        // Credit به Wallet
        // IdempotencyKey حداکثر 100 کاراکتر است، پس الگو را کوتاه نگه می‌داریم
        var idempotencyKey = $"manual-deposit-{userId}-{DateTime.Now:yyyyMMddHHmmss}";
        var creditResult = await _walletService.CreditAsync(
            userId,
            amountRial,
            operationType.Id,
            referenceType.Id,
            null,
            idempotencyKey,
            !string.IsNullOrWhiteSpace(description) ? description.Trim() : "واریز دستی توسط مدیر");

        if (!creditResult.Success)
            throw new AppException(creditResult.ErrorMessage ?? "خطا در واریز وجه");

        // اگر این واریز بابت تسویهٔ واقعی با کاربر است، روی رکورد تراکنش علامت بزنیم
        if (isBankSettlement && creditResult.TransactionId != Guid.Empty)
        {
            var tx = await _db.WalletTransactions
                .FirstOrDefaultAsync(t => t.Id == creditResult.TransactionId);

            if (tx != null)
            {
                tx.IsBankSettlement = true;
                await _db.SaveChangesAsync();
            }
        }

        return creditResult;
    }

    public async Task<WalletTransactionResultDto> ManualWithdrawalAsync(
        Guid userId,
        long amountRial,
        string description,
        bool isBankSettlement = false)
    {
        if (amountRial <= 0)
            throw new ValidationException(new() { "مبلغ برداشت باید بیشتر از صفر باشد." });

        // بررسی وجود کاربر
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new NotFoundException("کاربر یافت نشد.");

        // دریافت OperationType و ReferenceType
        var operationType = await _db.FinancialOperationTypes
            .FirstOrDefaultAsync(ot => ot.Code == "ManualWithdrawal");

        if (operationType == null)
            throw new AppException("نوع عملیات 'ManualWithdrawal' یافت نشد. لطفاً با مدیر سیستم تماس بگیرید.");

        var referenceType = await _db.FinancialReferenceTypes
            .FirstOrDefaultAsync(rt => rt.Code == "AdminAction");

        if (referenceType == null)
            throw new AppException("نوع مرجع 'AdminAction' یافت نشد. لطفاً با مدیر سیستم تماس بگیرید.");

        // Debit از Wallet (اجازه موجودی منفی برای بدهکار/بستانکار)
        // IdempotencyKey حداکثر 100 کاراکتر است، پس الگو را کوتاه نگه می‌داریم
        var idempotencyKey = $"manual-withdrawal-{userId}-{DateTime.Now:yyyyMMddHHmmss}";
        var debitResult = await _walletService.DebitAllowNegativeAsync(
            userId,
            amountRial,
            operationType.Id,
            referenceType.Id,
            null,
            idempotencyKey,
            !string.IsNullOrWhiteSpace(description) ? description.Trim() : "برداشت دستی توسط مدیر");

        if (!debitResult.Success)
            throw new AppException(debitResult.ErrorMessage ?? "خطا در برداشت وجه");

        // اگر این برداشت بابت تسویهٔ واقعی با کاربر است، روی رکورد تراکنش علامت بزنیم
        if (isBankSettlement && debitResult.TransactionId != Guid.Empty)
        {
            var tx = await _db.WalletTransactions
                .FirstOrDefaultAsync(t => t.Id == debitResult.TransactionId);

            if (tx != null)
            {
                tx.IsBankSettlement = true;
                await _db.SaveChangesAsync();
            }
        }

        return debitResult;
    }

    /// <summary>
    /// گزارش دفتر کل تراکنش‌های کیف پول با فیلترهای مختلف (برای مانیتورینگ مالی)
    /// </summary>
    public async Task<WalletTransactionsReportDto> GetAllTransactionsAsync(WalletTransactionListQueryDto query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var baseQuery =
            from t in _db.WalletTransactions
                .AsNoTracking()
                .Include(x => x.Direction)
                .Include(x => x.OperationType)
                .Include(x => x.Status)
                .Include(x => x.ReferenceType)
            join w in _db.Wallets.AsNoTracking() on t.WalletId equals w.Id
            join u in _db.Users.AsNoTracking() on w.UserId equals u.Id
            select new
            {
                Transaction = t,
                Wallet = w,
                User = u
            };

        // فیلترها
        if (query.UserId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Wallet.UserId == query.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.PhoneNumber))
        {
            baseQuery = baseQuery.Where(x => x.User.PhoneNumber.Contains(query.PhoneNumber));
        }

        if (!string.IsNullOrWhiteSpace(query.DisplayName))
        {
            baseQuery = baseQuery.Where(x => x.User.DisplayName != null &&
                                             x.User.DisplayName.Contains(query.DisplayName));
        }

        if (!string.IsNullOrWhiteSpace(query.DirectionCode))
        {
            baseQuery = baseQuery.Where(x => x.Transaction.Direction.Code == query.DirectionCode);
        }

        if (!string.IsNullOrWhiteSpace(query.OperationTypeCode))
        {
            baseQuery = baseQuery.Where(x => x.Transaction.OperationType.Code == query.OperationTypeCode);
        }

        if (!string.IsNullOrWhiteSpace(query.StatusCode))
        {
            baseQuery = baseQuery.Where(x => x.Transaction.Status.Code == query.StatusCode);
        }

        if (!string.IsNullOrWhiteSpace(query.ReferenceTypeCode))
        {
            baseQuery = baseQuery.Where(x => x.Transaction.ReferenceType.Code == query.ReferenceTypeCode);
        }

        if (query.SourceCategory != null)
        {
            var src = query.SourceCategory;

            if (string.Equals(src, "Bank", StringComparison.OrdinalIgnoreCase))
            {
                // تراکنش‌های بانکی:
                // 1) شارژ از طریق درگاه (TopUp + Payment)
                // 2) واریز/برداشت دستی که به‌عنوان تسویهٔ بانکی علامت خورده است
                baseQuery = baseQuery.Where(x =>
                    (x.Transaction.OperationType.Code == "TopUp" &&
                     x.Transaction.ReferenceType.Code == "Payment") ||
                    ((x.Transaction.OperationType.Code == "ManualDeposit" ||
                      x.Transaction.OperationType.Code == "ManualWithdrawal") &&
                     x.Transaction.ReferenceType.Code == "AdminAction" &&
                     x.Transaction.IsBankSettlement));
            }
            else if (string.Equals(src, "Commission", StringComparison.OrdinalIgnoreCase))
            {
                // تراکنش‌های پورسانت: CommissionEarned با مرجع WalletTransaction
                baseQuery = baseQuery.Where(x =>
                    x.Transaction.OperationType.Code == "CommissionEarned" &&
                    x.Transaction.ReferenceType.Code == "WalletTransaction");
            }
            else if (string.Equals(src, "Manual", StringComparison.OrdinalIgnoreCase))
            {
                // عملیات دستی مدیر (تغییرات داخلی، غیر از تسویه‌های بانکی)
                baseQuery = baseQuery.Where(x =>
                    (x.Transaction.OperationType.Code == "ManualDeposit" ||
                     x.Transaction.OperationType.Code == "ManualWithdrawal") &&
                    x.Transaction.ReferenceType.Code == "AdminAction" &&
                    !x.Transaction.IsBankSettlement);
            }
            else if (string.Equals(src, "Other", StringComparison.OrdinalIgnoreCase))
            {
                // سایر تراکنش‌ها (غیر از Bank، Commission، Manual)
                baseQuery = baseQuery.Where(x =>
                    !((x.Transaction.OperationType.Code == "TopUp" &&
                       x.Transaction.ReferenceType.Code == "Payment") ||
                      ((x.Transaction.OperationType.Code == "ManualDeposit" ||
                        x.Transaction.OperationType.Code == "ManualWithdrawal") &&
                       x.Transaction.ReferenceType.Code == "AdminAction" &&
                       x.Transaction.IsBankSettlement) ||
                      (x.Transaction.OperationType.Code == "CommissionEarned" &&
                       x.Transaction.ReferenceType.Code == "WalletTransaction") ||
                      ((x.Transaction.OperationType.Code == "ManualDeposit" ||
                        x.Transaction.OperationType.Code == "ManualWithdrawal") &&
                       x.Transaction.ReferenceType.Code == "AdminAction" &&
                       !x.Transaction.IsBankSettlement)));
            }
        }

        if (query.FromDate.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Transaction.CreatedAt >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            // تا انتهای روز
            var to = query.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            baseQuery = baseQuery.Where(x => x.Transaction.CreatedAt <= to);
        }

        // محاسبه مجموع‌ها روی کل داده‌های فیلتر شده
        var totalsQuery = baseQuery;

        var totalCreditRial = await totalsQuery
            .Where(x => x.Transaction.Direction.Code == "Credit")
            .SumAsync(x => (long?)x.Transaction.AmountRial) ?? 0;

        var totalDebitRial = await totalsQuery
            .Where(x => x.Transaction.Direction.Code == "Debit")
            .SumAsync(x => (long?)x.Transaction.AmountRial) ?? 0;

        // صفحه‌بندی
        var orderedQuery = baseQuery
            .OrderByDescending(x => x.Transaction.CreatedAt)
            .ThenByDescending(x => x.Transaction.Id);

        var totalCount = await orderedQuery.CountAsync();

        var items = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new WalletTransactionListItemDto
            {
                TransactionId = x.Transaction.Id,
                UserId = x.Wallet.UserId,
                PhoneNumber = x.User.PhoneNumber,
                DisplayName = x.User.DisplayName,
                DirectionCode = x.Transaction.Direction.Code,
                DirectionTitle = x.Transaction.Direction.Title,
                OperationTypeCode = x.Transaction.OperationType.Code,
                OperationTypeTitle = x.Transaction.OperationType.Title,
                StatusCode = x.Transaction.Status.Code,
                StatusTitle = x.Transaction.Status.Title,
                ReferenceTypeCode = x.Transaction.ReferenceType.Code,
                ReferenceTypeTitle = x.Transaction.ReferenceType.Title,
                AmountRial = x.Transaction.AmountRial,
                Description = x.Transaction.Description,
                CreatedAt = x.Transaction.CreatedAt,
                SourceCategory =
                    // بانکی
                    ((x.Transaction.OperationType.Code == "TopUp" &&
                      x.Transaction.ReferenceType.Code == "Payment") ||
                     ((x.Transaction.OperationType.Code == "ManualDeposit" ||
                       x.Transaction.OperationType.Code == "ManualWithdrawal") &&
                      x.Transaction.ReferenceType.Code == "AdminAction" &&
                      x.Transaction.IsBankSettlement))
                        ? "Bank"
                    // پورسانت
                    : x.Transaction.OperationType.Code == "CommissionEarned" &&
                      x.Transaction.ReferenceType.Code == "WalletTransaction"
                        ? "Commission"
                    // عملیات دستی داخلی (غیربانکی)
                    : (x.Transaction.OperationType.Code == "ManualDeposit" ||
                       x.Transaction.OperationType.Code == "ManualWithdrawal") &&
                      x.Transaction.ReferenceType.Code == "AdminAction" &&
                      !x.Transaction.IsBankSettlement
                        ? "Manual"
                    : "Other"
            })
            .ToListAsync();

        var paged = new PagedResult<WalletTransactionListItemDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };

        return new WalletTransactionsReportDto
        {
            Transactions = paged,
            TotalCreditRial = totalCreditRial,
            TotalDebitRial = totalDebitRial
        };
    }

    /// <summary>
    /// گزارش درآمد/هزینه
    /// </summary>
    public async Task<IncomeExpenseReportDto> GetIncomeExpenseReportAsync(IncomeExpenseReportQueryDto query)
    {
        var baseQuery = _db.WalletTransactions
            .AsNoTracking()
            .Include(x => x.Direction)
            .Include(x => x.OperationType)
            .Include(x => x.ReferenceType)
            .AsQueryable();

        // فیلتر تاریخ
        if (query.FromDate.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.CreatedAt >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            var to = query.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            baseQuery = baseQuery.Where(x => x.CreatedAt <= to);
        }

        // ============================================
        // محاسبه درآمدها (فقط پول واقعی که به حساب شرکت می‌آید)
        // ============================================
        // 1. شارژ کیف پول از طریق درگاه (TopUp + Payment)
        // کاربر پول به حساب شرکت واریز می‌کند → درآمد واقعی
        var topUpIncome = await baseQuery
            .Where(x => x.OperationType.Code == "TopUp" &&
                       x.ReferenceType.Code == "Payment" &&
                       x.Direction.Code == "Credit" &&
                       x.Status.Code == "Committed")
            .SumAsync(x => (long?)x.AmountRial) ?? 0;

        // 2. واریز دستی بانکی (ManualDeposit + IsBankSettlement)
        // مدیر پول به حساب شرکت واریز می‌کند → درآمد واقعی
        var manualDepositIncome = await baseQuery
            .Where(x => x.OperationType.Code == "ManualDeposit" &&
                       x.ReferenceType.Code == "AdminAction" &&
                       x.IsBankSettlement &&
                       x.Direction.Code == "Credit" &&
                       x.Status.Code == "Committed")
            .SumAsync(x => (long?)x.AmountRial) ?? 0;

        // توجه: خرید اشتراک (SubscriptionPurchase) و باز کردن تماس (UnlockContactFee)
        // تراکنش‌های داخلی هستند و نباید به عنوان درآمد محاسبه شوند
        // چون کاربر از موجودی کیف پولش استفاده می‌کند (پول قبلاً شارژ شده بود)

        var totalIncomeRial = topUpIncome + manualDepositIncome;

        var incomeTransactionCount = await baseQuery
            .Where(x => 
                (x.OperationType.Code == "TopUp" && x.ReferenceType.Code == "Payment" && x.Direction.Code == "Credit" && x.Status.Code == "Committed") ||
                (x.OperationType.Code == "ManualDeposit" && x.ReferenceType.Code == "AdminAction" && x.IsBankSettlement && x.Direction.Code == "Credit" && x.Status.Code == "Committed"))
            .CountAsync();

        // ============================================
        // محاسبه هزینه‌ها (فقط پول واقعی که از حساب شرکت خارج می‌شود)
        // ============================================
        // فقط برداشت دستی بانکی (ManualWithdrawal + IsBankSettlement)
        // مدیر واقعاً پول از حساب شرکت به حساب کاربر واریز می‌کند → هزینه واقعی
        var manualWithdrawalExpense = await baseQuery
            .Where(x => x.OperationType.Code == "ManualWithdrawal" &&
                       x.ReferenceType.Code == "AdminAction" &&
                       x.IsBankSettlement &&
                       x.Direction.Code == "Debit" &&
                       x.Status.Code == "Committed")
            .SumAsync(x => (long?)x.AmountRial) ?? 0;

        // توجه: 
        // - پورسانت‌ها (CommissionEarned): تراکنش داخلی است. پول به کیف پول بازاریاب واریز می‌شود
        //   اما از حساب بانکی شرکت خارج نمی‌شود. فقط وقتی بازاریاب برداشت کند (ManualWithdrawal + IsBankSettlement)
        //   آن وقت هزینه واقعی محسوب می‌شود.
        // - باز کردن اطلاعات تماس (UnlockContactFee) و خرید اشتراک (SubscriptionPurchase):
        //   تراکنش‌های داخلی هستند و نباید به عنوان هزینه محاسبه شوند
        //   چون کاربر از موجودی کیف پولش استفاده می‌کند (پول قبلاً شارژ شده بود)

        var totalExpenseRial = manualWithdrawalExpense;

        var expenseTransactionCount = await baseQuery
            .Where(x => x.OperationType.Code == "ManualWithdrawal" &&
                       x.ReferenceType.Code == "AdminAction" &&
                       x.IsBankSettlement &&
                       x.Direction.Code == "Debit" &&
                       x.Status.Code == "Committed")
            .CountAsync();

        // ============================================
        // دسته‌بندی درآمدها (فقط درآمدهای واقعی)
        // ============================================
        var incomeCategories = new List<IncomeExpenseCategoryDto>
        {
            new IncomeExpenseCategoryDto
            {
                CategoryName = "شارژ کیف پول (درگاه پرداخت)",
                TotalAmountRial = topUpIncome,
                TransactionCount = await baseQuery
                    .Where(x => x.OperationType.Code == "TopUp" &&
                               x.ReferenceType.Code == "Payment" &&
                               x.Direction.Code == "Credit" &&
                               x.Status.Code == "Committed")
                    .CountAsync()
            },
            new IncomeExpenseCategoryDto
            {
                CategoryName = "واریز دستی بانکی",
                TotalAmountRial = manualDepositIncome,
                TransactionCount = await baseQuery
                    .Where(x => x.OperationType.Code == "ManualDeposit" &&
                               x.ReferenceType.Code == "AdminAction" &&
                               x.IsBankSettlement &&
                               x.Direction.Code == "Credit" &&
                               x.Status.Code == "Committed")
                    .CountAsync()
            }
        };

        // ============================================
        // دسته‌بندی هزینه‌ها (فقط هزینه‌های واقعی)
        // ============================================
        var expenseCategories = new List<IncomeExpenseCategoryDto>
        {
            new IncomeExpenseCategoryDto
            {
                CategoryName = "برداشت دستی بانکی",
                TotalAmountRial = manualWithdrawalExpense,
                TransactionCount = await baseQuery
                    .Where(x => x.OperationType.Code == "ManualWithdrawal" &&
                               x.ReferenceType.Code == "AdminAction" &&
                               x.IsBankSettlement &&
                               x.Direction.Code == "Debit" &&
                               x.Status.Code == "Committed")
                    .CountAsync()
            }
        };

        return new IncomeExpenseReportDto
        {
            TotalIncomeRial = totalIncomeRial,
            TotalExpenseRial = totalExpenseRial,
            IncomeTransactionCount = incomeTransactionCount,
            ExpenseTransactionCount = expenseTransactionCount,
            IncomeCategories = incomeCategories,
            ExpenseCategories = expenseCategories
        };
    }

    /// <summary>
    /// داشبورد مالی - خلاصه کلی وضعیت مالی سیستم
    /// </summary>
    public async Task<FinancialDashboardDto> GetFinancialDashboardAsync()
    {
        var baseQuery = _db.WalletTransactions
            .AsNoTracking()
            .Include(x => x.Direction)
            .Include(x => x.OperationType)
            .Include(x => x.ReferenceType)
            .Include(x => x.Status)
            .AsQueryable();

        // ============================================
        // درآمد واقعی (پول واقعاً به حساب شرکت آمده)
        // ============================================
        var realIncome = await baseQuery
            .Where(x =>
                ((x.OperationType.Code == "TopUp" && x.ReferenceType.Code == "Payment") ||
                 (x.OperationType.Code == "ManualDeposit" && x.ReferenceType.Code == "AdminAction" && x.IsBankSettlement)) &&
                x.Direction.Code == "Credit" &&
                x.Status.Code == "Committed")
            .SumAsync(x => (long?)x.AmountRial) ?? 0;

        var realIncomeCount = await baseQuery
            .Where(x =>
                ((x.OperationType.Code == "TopUp" && x.ReferenceType.Code == "Payment") ||
                 (x.OperationType.Code == "ManualDeposit" && x.ReferenceType.Code == "AdminAction" && x.IsBankSettlement)) &&
                x.Direction.Code == "Credit" &&
                x.Status.Code == "Committed")
            .CountAsync();

        // ============================================
        // هزینه واقعی (پول واقعاً از حساب شرکت خارج شده)
        // ============================================
        var realExpense = await baseQuery
            .Where(x => x.OperationType.Code == "ManualWithdrawal" &&
                       x.ReferenceType.Code == "AdminAction" &&
                       x.IsBankSettlement &&
                       x.Direction.Code == "Debit" &&
                       x.Status.Code == "Committed")
            .SumAsync(x => (long?)x.AmountRial) ?? 0;

        var realExpenseCount = await baseQuery
            .Where(x => x.OperationType.Code == "ManualWithdrawal" &&
                       x.ReferenceType.Code == "AdminAction" &&
                       x.IsBankSettlement &&
                       x.Direction.Code == "Debit" &&
                       x.Status.Code == "Committed")
            .CountAsync();

        // ============================================
        // پورسانت‌ها (تراکنش داخلی)
        // ============================================
        var totalCommissions = await baseQuery
            .Where(x => x.OperationType.Code == "CommissionEarned" &&
                       x.Direction.Code == "Credit" &&
                       x.Status.Code == "Committed")
            .SumAsync(x => (long?)x.AmountRial) ?? 0;

        var commissionCount = await baseQuery
            .Where(x => x.OperationType.Code == "CommissionEarned" &&
                       x.Direction.Code == "Credit" &&
                       x.Status.Code == "Committed")
            .CountAsync();

        // ============================================
        // خرید اشتراک (تراکنش داخلی)
        // ============================================
        var totalSubscriptions = await baseQuery
            .Where(x => x.OperationType.Code == "SubscriptionPurchase" &&
                       x.Direction.Code == "Debit" &&
                       x.Status.Code == "Committed")
            .SumAsync(x => (long?)x.AmountRial) ?? 0;

        var subscriptionCount = await baseQuery
            .Where(x => x.OperationType.Code == "SubscriptionPurchase" &&
                       x.Direction.Code == "Debit" &&
                       x.Status.Code == "Committed")
            .CountAsync();

        // ============================================
        // باز کردن اطلاعات تماس (تراکنش داخلی)
        // ============================================
        var totalUnlockContacts = await baseQuery
            .Where(x => x.OperationType.Code == "UnlockContactFee" &&
                       x.Direction.Code == "Debit" &&
                       x.Status.Code == "Committed")
            .SumAsync(x => (long?)x.AmountRial) ?? 0;

        var unlockContactCount = await baseQuery
            .Where(x => x.OperationType.Code == "UnlockContactFee" &&
                       x.Direction.Code == "Debit" &&
                       x.Status.Code == "Committed")
            .CountAsync();

        // ============================================
        // مجموع موجودی کیف پول‌ها
        // ============================================
        var totalWalletBalance = await _db.Wallets
            .AsNoTracking()
            .Where(w => w.WalletTypeId == 1) // Main Wallet
            .SumAsync(w => (long?)w.BalanceRial) ?? 0;

        // تعداد کاربران دارای کیف پول با تراکنش (گردش حساب)
        var walletUserWithTransactionCount = await _db.WalletTransactions
            .AsNoTracking()
            .Where(t => t.Status.Code == "Committed")
            .Select(t => t.Wallet.UserId)
            .Distinct()
            .CountAsync();


        // ============================================
        // تعداد تراکنش‌های داخلی
        // ============================================
        var internalTransactionCount = await baseQuery
            .Where(x =>
                (x.OperationType.Code == "CommissionEarned" ||
                 x.OperationType.Code == "SubscriptionPurchase" ||
                 x.OperationType.Code == "UnlockContactFee") &&
                x.Status.Code == "Committed")
            .CountAsync();

        return new FinancialDashboardDto
        {
            TotalRealIncomeRial = realIncome,
            RealIncomeTransactionCount = realIncomeCount,
            TotalRealExpenseRial = realExpense,
            RealExpenseTransactionCount = realExpenseCount,
            TotalCommissionsRial = totalCommissions,
            CommissionCount = commissionCount,
            TotalSubscriptionPurchasesRial = totalSubscriptions,
            SubscriptionPurchaseCount = subscriptionCount,
            TotalUnlockContactFeesRial = totalUnlockContacts,
            UnlockContactCount = unlockContactCount,
            TotalWalletBalanceRial = totalWalletBalance,
            WalletUserWithTransactionCount = walletUserWithTransactionCount,
            InternalTransactionCount = internalTransactionCount
        };
    }
}

