using IngApp.Application.Common.Interfaces.Authentication;
using IngApp.Domain.Entities.Auth;
using IngApp.Domain.Enums;
using IngApp.Infrastructure.Common.Hashing;
using IngApp.Infrastructure.Services.Sms;

namespace IngApp.Infrastructure.Services.Auth;

public class OtpService : IOtpService
{
    private readonly IOtpCodeRepository _repository;
    private readonly SmsIrSender _smsSender;

    // محدودیت‌ها
    private const int MaxWrongAttempts = 5;
    private const int BlockMinutes = 3;

    public OtpService(IOtpCodeRepository repository, SmsIrSender smsSender)
    {
        _repository = repository;
        _smsSender = smsSender;
    }

    public async Task<string> GenerateCodeAsync(string phoneNumber)
    {
        var latest = await _repository.GetLatestActiveOtpAsync(phoneNumber, OtpPurpose.Login);

        // جلوگیری از درخواست پشت‌سرهم OTP
        //if (latest != null && (DateTime.UtcNow - latest.CreatedAtUtc).TotalSeconds < 60)
        //    throw new Exception("لطفاً کمی بعد دوباره تلاش کنید.");

        if (latest != null && (DateTime.UtcNow - latest.CreatedAtUtc) < TimeSpan.FromMinutes(1))
        {
            // اگر کمتر از یک دقیقه از آخرین ارسال گذشته، دیگه Exception نده.
            // فقط اجازه بده از همان کدی که قبلاً ارسال شده استفاده شود.
            // چون متد در عمل نتیجه‌اش در API استفاده نمی‌شود، یک مقدار خنثی برمی‌گردانیم.
            return string.Empty;
        }

        // OTP جدید
        var code = new Random().Next(100000, 999999).ToString();

        var otp = new OtpCode
        {
            Id = Guid.NewGuid(),
            PhoneNumber = phoneNumber,
            CodeHash = Sha256Hash.Hash(code),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            Purpose = OtpPurpose.Login
        };

        await _repository.AddAsync(otp);
        await _repository.SaveChangesAsync();

        await _smsSender.SendOtpAsync(phoneNumber, code);

        return code;
    }

    public async Task<(bool Success, string Message)> ValidateCodeAsync(string phoneNumber, string code)
    {
        var otp = await _repository.GetLatestActiveOtpAsync(phoneNumber, OtpPurpose.Login);
        if (otp == null)
            return (false, "کد معتبر نیست.");

        // بلاک شدن به دلیل تلاش‌های زیاد
        if (otp.AttemptCount >= MaxWrongAttempts &&
            otp.LastAttemptAtUtc.HasValue &&
            otp.LastAttemptAtUtc.Value.AddMinutes(BlockMinutes) > DateTime.UtcNow)
        {
            return (false, "تعداد تلاش بیش از حد مجاز است. لطفاً چند دقیقه بعد دوباره تلاش کنید.");
        }

        otp.RegisterAttempt();

        if (otp.IsExpired())
        {
            await _repository.SaveChangesAsync();
            return (false, "کد منقضی شده است.");
        }

        if (otp.CodeHash != Sha256Hash.Hash(code))
        {
            await _repository.SaveChangesAsync();
            return (false, "کد وارد شده صحیح نیست.");
        }

        otp.MarkAsUsed();
        await _repository.SaveChangesAsync();

        return (true, "موفق");
    }
}
