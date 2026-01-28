using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IngApp.Application.Common.Interfaces.Products;

public interface IProductFileStorageService
{
    /// <summary>
    /// ذخیره فایل تصویر Product و برگرداندن مسیر نسبی برای ذخیره در DB
    /// </summary>
    Task<string> SaveAsync(
        int productId,
        string originalFileName,
        Stream fileStream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// گرفتن مسیر فیزیکی فایل بر اساس مسیر نسبی ذخیره شده در DB.
    /// </summary>
    /// <param name="relativePath">مسیر نسبی ذخیره‌شده در دیتابیس</param>
    /// <param name="fullPath">مسیر فیزیکی کامل روی دیسک</param>
    /// <param name="contentType">نوع محتوا (MIME type)</param>
    /// <returns>اگر فایل وجود داشته باشد true</returns>
    bool TryGetFileInfo(string relativePath, out string fullPath, out string contentType);
}












