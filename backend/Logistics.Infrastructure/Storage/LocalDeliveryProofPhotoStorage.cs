using Logistics.Application.Ports;
using Microsoft.AspNetCore.Hosting;

namespace Logistics.Infrastructure.Storage;

public sealed class LocalDeliveryProofPhotoStorage : IDeliveryProofPhotoStorage
{
    private readonly IWebHostEnvironment _env;

    public LocalDeliveryProofPhotoStorage(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveAsync(
        string clientProofId,
        byte[] photoBytes,
        string? fileName,
        CancellationToken ct)
    {
        var ext = string.IsNullOrWhiteSpace(fileName)
            ? ".jpg"
            : Path.GetExtension(fileName);

        if (string.IsNullOrWhiteSpace(ext))
            ext = ".jpg";

        var safeExt = ext.StartsWith('.') ? ext : $".{ext}";
        var webRoot = _env.WebRootPath ?? _env.ContentRootPath;

        var targetDir = Path.Combine(webRoot, "uploads", "delivery-proofs");
        Directory.CreateDirectory(targetDir);

        var safeName = $"{clientProofId}{safeExt}";
        var fullPath = Path.Combine(targetDir, safeName);

        await File.WriteAllBytesAsync(fullPath, photoBytes, ct);

        // If static files are enabled, clients can fetch it from this URL.
        return $"/uploads/delivery-proofs/{safeName}";
    }
}

