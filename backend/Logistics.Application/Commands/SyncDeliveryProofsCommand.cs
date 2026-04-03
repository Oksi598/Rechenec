using MediatR;

namespace Logistics.Application.Commands;

public sealed record SyncDeliveryProofsCommand(
    Guid OrderId,
    string ClientProofId,
    string Signature,
    byte[] PhotoBytes,
    string? PhotoFileName) : IRequest<bool>;

