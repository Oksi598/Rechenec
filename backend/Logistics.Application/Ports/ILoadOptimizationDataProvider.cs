using Logistics.Domain.Entities;

namespace Logistics.Application.Ports;

public interface ILoadOptimizationDataProvider
{
    Task<LoadOptimizationData> GetDataAsync(Guid routeId, CancellationToken ct);
}

/// <summary>WGS84 coordinates for Haversine distance (lat/lon in decimal degrees).</summary>
public readonly record struct GeoCoordinate(double Latitude, double Longitude);

public sealed record LoadOptimizationData(
    Vehicle Vehicle,
    GeoCoordinate Depot,
    IList<Order> Orders);
