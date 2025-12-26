using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IngApp.Application.Common.Interfaces.Offers;
using Microsoft.Extensions.Configuration;

namespace IngApp.Infrastructure.Services.Offers;

public class OfferFileStorageService : IOfferFileStorageService
{
    private readonly string _rootPath;

    public OfferFileStorageService(IConfiguration configuration)
    {
        _rootPath = configuration["OfferFileStorage:RootPath"]
            ?? throw new Exception("OfferFileStorage:RootPath is not configured.");
    }

    public async Task<string> SaveAsync(
        Guid supplierUserId,
        int offerId,
        string originalFileName,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        if (fileStream == null || !fileStream.CanRead)
            throw new ArgumentException("Invalid file stream.", nameof(fileStream));

        var offerFolder = Path.Combine(
            _rootPath,
            supplierUserId.ToString(),
            offerId.ToString());

        if (!Directory.Exists(offerFolder))
            Directory.CreateDirectory(offerFolder);

        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(ext))
            ext = ".bin";

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(offerFolder, fileName);

        await using (var output = File.Create(fullPath))
        {
            await fileStream.CopyToAsync(output, cancellationToken);
        }

        // مسیر نسبی برای DB
        var relativePath = $"{supplierUserId}/{offerId}/{fileName}";
        return relativePath;
    }
}
