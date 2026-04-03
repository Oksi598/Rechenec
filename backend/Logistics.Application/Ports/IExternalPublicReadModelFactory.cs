using Logistics.Application.Dtos;

namespace Logistics.Application.Ports;

public interface IExternalPublicReadModelFactory
{
    Task<ExternalContactDto> CreateAsync(Guid externalContactId, MaskingScope scope, CancellationToken ct);
}

