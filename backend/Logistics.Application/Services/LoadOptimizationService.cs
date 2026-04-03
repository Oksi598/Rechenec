using Logistics.Domain.Entities;
using Logistics.Domain.Enums;
using Logistics.Application.Ports;

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

        if (data.Vehicle.CapacityWeight <= 0 && data.Vehicle.CapacityVolume <= 0)
            throw new InvalidOperationException("Vehicle capacities are not configured.");

        static double HaversineKm(GeoCoordinate from, GeoCoordinate to)
        {
            static double ToRad(double deg) => deg * Math.PI / 180.0;

            var lat1 = ToRad(from.Latitude);
            var lon1 = ToRad(from.Longitude);
            var lat2 = ToRad(to.Latitude);
            var lon2 = ToRad(to.Longitude);

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

        var now = DateTimeOffset.UtcNow;

        var nonUrgent = data.Orders
            .Where(o => o.Priority != 1)
            .Select(o => new
            {
                Order = o,
                DistanceKm = HaversineKm(
                    data.Depot,
                    new GeoCoordinate(o.DeliveryLatitude, o.DeliveryLongitude)),
                PriorityScore = ComputePriorityScore(o.Priority),
                SlaScore = ComputeSlaScore(o.CreatedAt, now),
                VehicleCompatibilityScore = ComputeVehicleCompatibilityScore(data.Vehicle.VehicleType, o.Weight, o.Volume)
            })
            .Select(x => new
            {
                x.Order,
                x.DistanceKm,
                x.PriorityScore,
                x.SlaScore,
                x.VehicleCompatibilityScore,
                CompositeScore = ComputeCompositeScore(
                    x.PriorityScore,
                    x.DistanceKm,
                    x.SlaScore,
                    x.VehicleCompatibilityScore)
            })
            // Stable deterministic ordering for repeatable planning results.
            .OrderByDescending(x => x.CompositeScore)
            .ThenBy(x => x.Order.Priority)
            .ThenBy(x => x.DistanceKm)
            .ThenBy(x => x.Order.CreatedAt)
            .ThenBy(x => x.Order.Id)
            .ToList();

        double loadedWeight = 0;
        double loadedVolume = 0;

        // Exception orders always go to ReadyForRouting, but still contribute to occupancy.
        foreach (var o in urgent)
        {
            loadedWeight += o.Weight;
            loadedVolume += o.Volume;

            if (o.Status != OrderStatus.ReadyForRouting)
                o.Status = OrderStatus.ReadyForRouting;
        }

        foreach (var x in nonUrgent)
        {
            var o = x.Order;

            loadedWeight += o.Weight;
            loadedVolume += o.Volume;

            // OR rule: weight OR volume must satisfy the threshold.
            var weightFill = data.Vehicle.CapacityWeight > 0
                ? loadedWeight / data.Vehicle.CapacityWeight
                : 0.0;

            var volumeFill = data.Vehicle.CapacityVolume > 0
                ? loadedVolume / data.Vehicle.CapacityVolume
                : 0.0;

            var isReadyAllowed = weightFill >= FillThreshold || volumeFill >= FillThreshold;

            o.Status = isReadyAllowed ? OrderStatus.ReadyForRouting : OrderStatus.PendingLoadOptimization;
        }

        await _uow.SaveChangesAsync(ct);
    }

    private static double ComputePriorityScore(int priority)
    {
        // Normalize 1..10 to 1.0..0.1 where lower numeric priority means higher urgency.
        var clamped = Math.Clamp(priority, 1, 10);
        return (11.0 - clamped) / 10.0;
    }

    private static double ComputeSlaScore(DateTimeOffset createdAt, DateTimeOffset now)
    {
        var ageHours = Math.Max(0.0, (now - createdAt).TotalHours);
        // Requests older than 24h have max SLA urgency weight.
        return Math.Min(1.0, ageHours / 24.0);
    }

    private static double ComputeVehicleCompatibilityScore(string vehicleType, double weight, double volume)
    {
        var type = (vehicleType ?? string.Empty).ToLowerInvariant();
        var heavyOrBulky = weight >= 1200 || volume >= 18;

        if (type.Contains("truck") || type.Contains("heavy"))
            return heavyOrBulky ? 1.0 : 0.8;

        if (type.Contains("van"))
            return heavyOrBulky ? 0.4 : 0.9;

        return heavyOrBulky ? 0.6 : 0.7;
    }

    private static double ComputeCompositeScore(
        double priorityScore,
        double distanceKm,
        double slaScore,
        double vehicleCompatibilityScore)
    {
        // Distance factor decays with route length to avoid over-prioritizing distant drops.
        var distanceScore = 1.0 / (1.0 + Math.Max(0.0, distanceKm));

        // Weighted sum:
        // - priority: 40%
        // - geo proximity: 25%
        // - SLA aging: 20%
        // - vehicle/order shape compatibility: 15%
        return
            (priorityScore * 0.40) +
            (distanceScore * 0.25) +
            (slaScore * 0.20) +
            (vehicleCompatibilityScore * 0.15);
    }
}

