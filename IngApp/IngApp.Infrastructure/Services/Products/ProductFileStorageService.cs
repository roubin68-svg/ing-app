using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IngApp.Application.Common.Interfaces.Products;
using Microsoft.Extensions.Configuration;

namespace IngApp.Infrastructure.Services.Products;

public class ProductFileStorageService : IProductFileStorageService
{
    private readonly string _rootPath;

    public ProductFileStorageService(IConfiguration configuration)
    {
        _rootPath = configuration["ProductFileStorage:RootPath"]
            ?? throw new InvalidOperationException("ProductFileStorage:RootPath is not configured in appsettings.json. Please add: \"ProductFileStorage\": { \"RootPath\": \"C:\\\\Projects\\\\IngAppData\\\\Products\" }");
        
        // ایجاد پوشه root در صورت عدم وجود
        if (!Directory.Exists(_rootPath))
        {
            try
            {
                Directory.CreateDirectory(_rootPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Cannot create ProductFileStorage root directory at '{_rootPath}': {ex.Message}", ex);
            }
        }
    }

    public async Task<string> SaveAsync(
        int productId,
        string originalFileName,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        if (fileStream == null || !fileStream.CanRead)
            throw new ArgumentException("Invalid file stream.", nameof(fileStream));

        var productFolder = Path.Combine(_rootPath, productId.ToString());

        if (!Directory.Exists(productFolder))
            Directory.CreateDirectory(productFolder);

        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(ext))
            ext = ".bin";

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(productFolder, fileName);

        await using (var output = File.Create(fullPath))
        {
            await fileStream.CopyToAsync(output, cancellationToken);
        }

        // مسیر نسبی برای DB: {productId}/{fileName}
        var relativePath = $"{productId}/{fileName}";
        return relativePath;
    }

    public bool TryGetFileInfo(string relativePath, out string fullPath, out string contentType)
    {
        fullPath = Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(fullPath))
        {
            contentType = "application/octet-stream";
            return false;
        }

        // تشخیص contentType بر اساس extension
        var ext = Path.GetExtension(fullPath).ToLowerInvariant();
        contentType = ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };

        return true;
    }
}










