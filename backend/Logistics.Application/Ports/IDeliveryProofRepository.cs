using Logistics.Domain.Entities;

namespace Logistics.Application.Ports;

public interface IDeliveryProofRepository
{
    Task<DeliveryProof?> GetByClientProofIdAsync(string clientProofId, CancellationToken ct);
    Task AddAsync(DeliveryProof proof, CancellationToken ct);
}

