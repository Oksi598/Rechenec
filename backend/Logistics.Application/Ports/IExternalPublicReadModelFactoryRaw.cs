using Logistics.Application.Dtos;

namespace Logistics.Application.Ports;

public interface IExternalPublicReadModelFactoryRaw
{
    Task<ExternalContactDto> CreateAsync(Guid externalContactId, CancellationToken ct);
}

