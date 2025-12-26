using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IngApp.Application.Common.Interfaces.Kyc;
using Microsoft.Extensions.Configuration;

namespace IngApp.Infrastructure.Services.Kyc
{
    public class KycFileStorageService : IKycFileStorageService
    {
        private readonly string _rootPath;

        public KycFileStorageService(IConfiguration configuration)
        {
            _rootPath = configuration["KycFileStorage:RootPath"]
                ?? throw new Exception("KycFileStorage:RootPath is not configured.");
        }

        /// <summary>
        /// ذخیره فایل و برگرداندن مسیر نسبی برای ذخیره در DB.
        /// مسیر نسبی به شکل {userId}/{guid}{ext} برمی‌گردد.
        /// </summary>
        public async Task<string> SaveAsync(
            Guid userId,
            string originalFileName,
            Stream fileStream,
            CancellationToken cancellationToken = default)
        {
            if (fileStream == null || !fileStream.CanRead)
                throw new ArgumentException("Invalid file stream.", nameof(fileStream));

            var userFolder = Path.Combine(_rootPath, userId.ToString());

            if (!Directory.Exists(userFolder))
                Directory.CreateDirectory(userFolder);

            var ext = Path.GetExtension(originalFileName);
            if (string.IsNullOrWhiteSpace(ext))
                ext = ".bin";

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(userFolder, fileName);

            await using (var output = File.Create(fullPath))
            {
                await fileStream.CopyToAsync(output, cancellationToken);
            }

            // مسیر نسبی‌ای که در DB ذخیره می‌شود
            var relativePath = $"{userId}/{fileName}";
            return relativePath;
        }

        /// <summary>
        /// گرفتن مسیر کامل فایل و یک content-type ساده بر اساس مسیر نسبی.
        /// </summary>
        public bool TryGetFileInfo(string relativePath, out string fullPath, out string contentType)
        {
            fullPath = Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(fullPath))
            {
                contentType = "application/octet-stream";
                return false;
            }

            // فعلاً ساده: همیشه octet-stream
            // اگر دوست داشتی بعداً می‌تونیم در API براساس پسوند، نوع بهتر تعیین کنیم.
            contentType = "application/octet-stream";
            return true;
        }
    }
}
