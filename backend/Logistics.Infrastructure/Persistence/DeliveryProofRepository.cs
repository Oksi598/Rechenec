using Logistics.Application.Ports;
using Logistics.Domain.Entities;
using Logistics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Persistence;

public sealed class DeliveryProofRepository : IDeliveryProofRepository
{
    private readonly TmsDbContext _db;

    public DeliveryProofRepository(TmsDbContext db)
    {
        _db = db;
    }

    public Task<DeliveryProof?> GetByClientProofIdAsync(string clientProofId, CancellationToken ct)
        => _db.DeliveryProofs.FirstOrDefaultAsync(x => x.ClientProofId == clientProofId, ct);

    public Task AddAsync(DeliveryProof proof, CancellationToken ct)
    {
        _db.DeliveryProofs.Add(proof);
        return Task.CompletedTask;
    }
}

