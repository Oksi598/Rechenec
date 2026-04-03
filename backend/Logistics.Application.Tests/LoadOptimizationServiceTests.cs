using Logistics.Application.Ports;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Enums;

namespace Logistics.Application.Tests;

public sealed class LoadOptimizationServiceTests
{
    [Fact]
    public async Task PriorityOne_Order_IsAlwaysReadyForRouting()
    {
        var order = CreateOrder(priority: 1, weight: 10, volume: 1);
        var data = CreateData(new[] { order }, vehicleWeight: 1000, vehicleVolume: 100);
        var service = new LoadOptimizationService(new StubDataProvider(data), new StubUnitOfWork());

        await service.OptimizeAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(OrderStatus.ReadyForRouting, order.Status);
    }

    [Fact]
    public async Task NonUrgent_Order_BelowThreshold_StaysPendingLoadOptimization()
    {
        var order = CreateOrder(priority: 3, weight: 100, volume: 5);
        var data = CreateData(new[] { order }, vehicleWeight: 1000, vehicleVolume: 100);
        var service = new LoadOptimizationService(new StubDataProvider(data), new StubUnitOfWork());

        await service.OptimizeAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(OrderStatus.PendingLoadOptimization, order.Status);
    }

    private static LoadOptimizationData CreateData(IEnumerable<Order> orders, double vehicleWeight, double vehicleVolume)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            CapacityWeight = vehicleWeight,
            CapacityVolume = vehicleVolume,
            VehicleType = "Truck"
        };

        var depot = new GeoCoordinate(50.4501, 30.5234);
        return new LoadOptimizationData(vehicle, depot, orders.ToList());
    }

    private static Order CreateOrder(int priority, double weight, double volume)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            Priority = priority,
            Weight = weight,
            Volume = volume,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-3),
            Status = OrderStatus.PendingAssignment,
            DeliveryLatitude = 50.4510,
            DeliveryLongitude = 30.5236
        };
    }

    private sealed class StubDataProvider : ILoadOptimizationDataProvider
    {
        private readonly LoadOptimizationData _data;
        public StubDataProvider(LoadOptimizationData data) => _data = data;
        public Task<LoadOptimizationData> GetDataAsync(Guid routeId, CancellationToken ct) => Task.FromResult(_data);
    }

    private sealed class StubUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(1);
    }
}
