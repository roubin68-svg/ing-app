using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IngApp.Application.Common.Interfaces.Offers;

public interface IOfferFileStorageService
{
    /// <summary>
    /// ذخیره فایل Offer و برگرداندن مسیر نسبی برای ذخیره در DB
    /// </summary>
    Task<string> SaveAsync(
        Guid supplierUserId,
        int offerId,
        string originalFileName,
        Stream fileStream,
        CancellationToken cancellationToken = default);
}
