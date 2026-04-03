using Logistics.Application.Dtos;

namespace Logistics.Application.Ports;

public interface IDriverPublicReadModelFactory
{
    Task<DriverPublicDto> CreateAsync(Guid driverId, MaskingScope scope, CancellationToken ct);
}

