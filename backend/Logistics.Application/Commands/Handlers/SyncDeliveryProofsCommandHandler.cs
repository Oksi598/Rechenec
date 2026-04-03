using Logistics.Application.Commands;
using Logistics.Application.Ports;
using Logistics.Domain.Entities;
using MediatR;

namespace Logistics.Application.Commands.Handlers;

public sealed class SyncDeliveryProofsCommandHandler : IRequestHandler<SyncDeliveryProofsCommand, bool>
{
    private readonly IDeliveryProofRepository _proofs;
    private readonly IDeliveryProofPhotoStorage _photoStorage;
    private readonly IUnitOfWork _uow;

    public SyncDeliveryProofsCommandHandler(
        IDeliveryProofRepository proofs,
        IDeliveryProofPhotoStorage photoStorage,
        IUnitOfWork uow)
    {
        _proofs = proofs;
        _photoStorage = photoStorage;
        _uow = uow;
    }

    public async Task<bool> Handle(SyncDeliveryProofsCommand request, CancellationToken cancellationToken)
    {
        if (request.PhotoBytes.Length == 0)
            throw new InvalidOperationException("Delivery proof photo is empty.");

        var existing = await _proofs.GetByClientProofIdAsync(request.ClientProofId, cancellationToken);
        if (existing is not null)
            return false; // idempotent retry

        var photoUrl = await _photoStorage.SaveAsync(
            request.ClientProofId,
            request.PhotoBytes,
            request.PhotoFileName,
            cancellationToken);

        var proof = new DeliveryProof
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            ClientProofId = request.ClientProofId,
            Signature = request.Signature,
            PhotoUrl = photoUrl,
            DeliveredAt = DateTimeOffset.UtcNow
        };

        await _proofs.AddAsync(proof, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return true;
    }
}

