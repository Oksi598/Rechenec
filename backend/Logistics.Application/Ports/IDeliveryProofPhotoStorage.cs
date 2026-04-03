namespace Logistics.Application.Ports;

public interface IDeliveryProofPhotoStorage
{
    /// <summary>
    /// Persists delivery proof photo and returns a URL/path that can be used by clients.
    /// </summary>
    Task<string> SaveAsync(
        string clientProofId,
        byte[] photoBytes,
        string? fileName,
        CancellationToken ct);
}

