using IngApp.Domain.Entities.Users;

namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// کیف پول کاربر
/// هر کاربر دقیقاً یک Wallet با WalletType=Main دارد.
/// Balance هرگز نباید منفی شود (enforced در Business Logic).
/// </summary>
public class Wallet
{
    public Guid Id { get; set; }

    /// <summary>
    /// کاربر مالک کیف پول
    /// </summary>
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// واحد پول (در فاز فعلی فقط IRR)
    /// </summary>
    public int CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;

    /// <summary>
    /// نوع کیف پول (در فاز فعلی فقط Main)
    /// </summary>
    public int WalletTypeId { get; set; }
    public WalletType WalletType { get; set; } = null!;

    /// <summary>
    /// موجودی کیف پول به ریال (>= 0)
    /// </summary>
    public long BalanceRial { get; set; } = 0;

    /// <summary>
    /// Concurrency Token برای جلوگیری از Race Condition در آپدیت Balance
    /// </summary>
    public byte[] RowVersion { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}

