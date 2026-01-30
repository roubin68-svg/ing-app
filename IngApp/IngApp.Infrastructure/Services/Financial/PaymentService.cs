using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Features.Financial.DTO;
using IngApp.Domain.Entities.Financial;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Financial;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly IWalletService _walletService;

    public PaymentService(AppDbContext db, IWalletService walletService)
    {
        _db = db;
        _walletService = walletService;
    }

    public async Task<TopUpRequestResultDto> CreateTopUpRequestAsync(Guid userId, long amountRial, int gatewayId, string idempotencyKey)
    {
        // بررسی Idempotency
        var existingPayment = await _db.Payments
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey);

        if (existingPayment != null)
        {
            var gateway = await _db.PaymentGateways.FirstAsync(pg => pg.Id == existingPayment.GatewayId);
            return new TopUpRequestResultDto
            {
                PaymentId = existingPayment.Id,
                GatewayCode = gateway.Code,
                GatewayTitle = gateway.Title,
                AmountRial = existingPayment.AmountRial,
                PaymentToken = existingPayment.Id.ToString() // برای Mock
            };
        }

        // بررسی Gateway
        var paymentGateway = await _db.PaymentGateways
            .FirstOrDefaultAsync(pg => pg.Id == gatewayId && pg.IsActive);

        if (paymentGateway == null)
        {
            throw new AppException("درگاه پرداخت انتخابی یافت نشد یا غیرفعال است.");
        }

        // بررسی مبلغ (حداقل 10,000 تومان = 100,000 ریال)
        if (amountRial < 100000)
        {
            throw new ValidationException(new() { "حداقل مبلغ شارژ 10,000 تومان است." });
        }

        // دریافت Status (Pending)
        var pendingStatus = await _db.PaymentStatuses
            .FirstAsync(ps => ps.Code == "Pending");

        // ایجاد Payment
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GatewayId = gatewayId,
            StatusId = pendingStatus.Id,
            AmountRial = amountRial,
            IdempotencyKey = idempotencyKey,
            Description = "شارژ کیف پول",
            CreatedAt = DateTime.Now
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        return new TopUpRequestResultDto
        {
            PaymentId = payment.Id,
            GatewayCode = paymentGateway.Code,
            GatewayTitle = paymentGateway.Title,
            AmountRial = amountRial,
            PaymentToken = payment.Id.ToString() // برای Mock Gateway
        };
    }

    public async Task<PaymentVerificationResultDto> VerifyPaymentAsync(Guid paymentId, string? gatewayTransactionId, string? gatewayResponseJson)
    {
        var payment = await _db.Payments
            .Include(p => p.Gateway)
            .Include(p => p.Status)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null)
        {
            return new PaymentVerificationResultDto
            {
                Success = false,
                ErrorMessage = "پرداخت یافت نشد."
            };
        }

        // اگر قبلاً تایید شده است
        if (payment.Status.Code == "Success")
        {
            var wallet = await _db.Wallets
                .FirstAsync(w => w.UserId == payment.UserId && w.WalletTypeId == 1);

            return new PaymentVerificationResultDto
            {
                Success = true,
                PaymentId = payment.Id,
                WalletTransactionId = payment.WalletTransactionId,
                NewBalanceRial = wallet.BalanceRial
            };
        }

        // برای Mock Gateway، همیشه موفق است
        if (payment.Gateway.Code == "Mock")
        {
            // دریافت OperationType و ReferenceType
            var operationType = await _db.FinancialOperationTypes
                .FirstAsync(ot => ot.Code == "TopUp");

            var referenceType = await _db.FinancialReferenceTypes
                .FirstAsync(rt => rt.Code == "Payment");

            var successStatus = await _db.PaymentStatuses
                .FirstAsync(ps => ps.Code == "Success");

            // Credit به Wallet
            var creditResult = await _walletService.CreditAsync(
                payment.UserId,
                payment.AmountRial,
                operationType.Id,
                referenceType.Id,
                payment.Id, // ReferenceId = PaymentId
                $"topup-{payment.Id}", // IdempotencyKey برای Credit
                "شارژ کیف پول از طریق درگاه پرداخت");

            if (!creditResult.Success)
            {
                var failedStatus = await _db.PaymentStatuses
                    .FirstAsync(ps => ps.Code == "Failed");

                payment.StatusId = failedStatus.Id;
                payment.CompletedAt = DateTime.Now;
                payment.UpdatedAt = DateTime.Now;
                payment.GatewayResponseJson = gatewayResponseJson;
                await _db.SaveChangesAsync();

                return new PaymentVerificationResultDto
                {
                    Success = false,
                    ErrorMessage = "خطا در شارژ کیف پول"
                };
            }

            // به‌روزرسانی Payment
            payment.StatusId = successStatus.Id;
            payment.WalletTransactionId = creditResult.TransactionId;
            payment.GatewayTransactionId = gatewayTransactionId ?? $"MOCK-{payment.Id}";
            payment.GatewayResponseJson = gatewayResponseJson;
            payment.CompletedAt = DateTime.Now;
            payment.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            var wallet = await _db.Wallets
                .FirstAsync(w => w.UserId == payment.UserId && w.WalletTypeId == 1);

            return new PaymentVerificationResultDto
            {
                Success = true,
                PaymentId = payment.Id,
                WalletTransactionId = creditResult.TransactionId,
                NewBalanceRial = wallet.BalanceRial
            };
        }

        // برای درگاه‌های واقعی (آینده)
        return new PaymentVerificationResultDto
        {
            Success = false,
            ErrorMessage = "درگاه پرداخت پشتیبانی نمی‌شود."
        };
    }

    public async Task<PaymentStatusDto?> GetPaymentStatusAsync(Guid paymentId)
    {
        var payment = await _db.Payments
            .Include(p => p.Gateway)
            .Include(p => p.Status)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null)
            return null;

        return new PaymentStatusDto
        {
            Id = payment.Id,
            StatusCode = payment.Status.Code,
            StatusTitle = payment.Status.Title,
            GatewayCode = payment.Gateway.Code,
            GatewayTitle = payment.Gateway.Title,
            AmountRial = payment.AmountRial,
            CreatedAt = payment.CreatedAt,
            CompletedAt = payment.CompletedAt
        };
    }

    public async Task<List<PaymentGatewayDto>> GetActiveGatewaysAsync()
    {
        return await _db.PaymentGateways
            .Where(pg => pg.IsActive)
            .Select(pg => new PaymentGatewayDto
            {
                Id = pg.Id,
                Code = pg.Code,
                Title = pg.Title,
                Description = pg.Description,
                IsActive = pg.IsActive
            })
            .ToListAsync();
    }
}












