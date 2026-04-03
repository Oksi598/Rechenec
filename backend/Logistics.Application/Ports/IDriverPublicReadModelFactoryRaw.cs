using Logistics.Application.Dtos;

namespace Logistics.Application.Ports;

public interface IDriverPublicReadModelFactoryRaw
{
    Task<DriverPublicDto> CreateAsync(Guid driverId, CancellationToken ct);
}

