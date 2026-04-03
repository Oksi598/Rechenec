using Logistics.Domain.Entities;
using Logistics.Domain.Enums;
using Logistics.Application.Ports;
using NetTopologySuite.Geometries;

namespace Logistics.Application.Services;

public sealed class LoadOptimizationService
{
    private const double EarthRadiusKm = 6371.0;
    private const double FillThreshold = 0.70;

    private readonly ILoadOptimizationDataProvider _dataProvider;
    private readonly IUnitOfWork _uow;

    public LoadOptimizationService(
        ILoadOptimizationDataProvider dataProvider,
        IUnitOfWork uow)
    {
        _dataProvider = dataProvider;
        _uow = uow;
    }

    /// <summary>
    /// Applies LTL packing rule:
    /// - Orders with Priority == 1 are always set to ReadyForRouting (exception).
    /// - For other orders, ReadyForRouting is allowed only if after including the order:
    ///   weightFill >= 70% OR volumeFill >= 70%.
    /// </summary>
    public async Task OptimizeAsync(Guid routeId, CancellationToken ct = default)
    {
        var data = await _dataProvider.GetDataAsync(routeId, ct);
        if (data.Orders.Count == 0)
            return;

        if (data.Vehicle.CapacityWeight <= 0m && data.Vehicle.CapacityVolume <= 0m)
            throw new InvalidOperationException("Vehicle capacities are not configured.");

        // Compute deterministic distances and process non-urgent orders in a stable order.
        static double HaversineKm(Point a, Point b)
        {
            // NTS Point uses (X=Lon, Y=Lat).
            static double ToRad(double deg) => deg * Math.PI / 180.0;

            var lat1 = ToRad(a.Y);
            var lon1 = ToRad(a.X);
            var lat2 = ToRad(b.Y);
            var lon2 = ToRad(b.X);

            var dLat = lat2 - lat1;
            var dLon = lon2 - lon1;

            var sinDLat = Math.Sin(dLat / 2.0);
            var sinDLon = Math.Sin(dLon / 2.0);

            var h = sinDLat * sinDLat + Math.Cos(lat1) * Math.Cos(lat2) * sinDLon * sinDLon;
            var c = 2.0 * Math.Asin(Math.Min(1.0, Math.Sqrt(h)));
            return EarthRadiusKm * c;
        }

        var urgent = data.Orders
            .Where(o => o.Priority == 1)
            .ToList();

        var nonUrgent = data.Orders
            .Where(o => o.Priority != 1)
            .Select(o => new
            {
                Order = o,
                DistanceKm = HaversineKm(data.DepotLocation, o.DeliveryLocation)
            })
            // Priority: lower value = higher urgency
            .OrderBy(x => x.Order.Priority)
            .ThenBy(x => x.DistanceKm)
            .ToList();

        double loadedWeight = 0;
        double loadedVolume = 0;

        // Exception orders always go to ReadyForRouting, but still contribute to occupancy.
        foreach (var o in urgent)
        {
            loadedWeight += (double)o.Weight;
            loadedVolume += (double)o.Volume;

            if (o.Status != OrderStatus.ReadyForRouting)
                o.Status = OrderStatus.ReadyForRouting;
        }

        foreach (var x in nonUrgent)
        {
            var o = x.Order;

            loadedWeight += (double)o.Weight;
            loadedVolume += (double)o.Volume;

            // OR rule: weight OR volume must satisfy the threshold.
            var weightFill = data.Vehicle.CapacityWeight > 0m
                ? loadedWeight / (double)data.Vehicle.CapacityWeight
                : 0.0;

            var volumeFill = data.Vehicle.CapacityVolume > 0m
                ? loadedVolume / (double)data.Vehicle.CapacityVolume
                : 0.0;

            var isReadyAllowed = weightFill >= FillThreshold || volumeFill >= FillThreshold;

            o.Status = isReadyAllowed ? OrderStatus.ReadyForRouting : OrderStatus.PendingLoadOptimization;
        }

        await _uow.SaveChangesAsync(ct);
    }
}

