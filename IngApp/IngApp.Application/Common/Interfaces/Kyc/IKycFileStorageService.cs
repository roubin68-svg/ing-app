using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IngApp.Application.Common.Interfaces.Kyc
{
    public interface IKycFileStorageService
    {
        /// <summary>
        /// ذخیره فایل KYC برای یک کاربر و برگرداندن مسیر نسبی فایل (برای ذخیره در DB)
        /// </summary>
        Task<string> SaveAsync(
            Guid userId,
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
}
