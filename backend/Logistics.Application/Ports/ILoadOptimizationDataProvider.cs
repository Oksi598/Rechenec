using Logistics.Domain.Entities;
using NetTopologySuite.Geometries;

namespace Logistics.Application.Ports;

public interface ILoadOptimizationDataProvider
{
    Task<LoadOptimizationData> GetDataAsync(Guid routeId, CancellationToken ct);
}

public sealed record LoadOptimizationData(
    Vehicle Vehicle,
    Point DepotLocation,
    IList<Order> Orders);

