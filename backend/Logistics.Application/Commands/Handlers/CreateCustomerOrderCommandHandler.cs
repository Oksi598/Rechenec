using Logistics.Domain.Entities;
using Logistics.Domain.Enums;
using Logistics.Application.Ports;
using Logistics.Application.Services;
using MediatR;

namespace Logistics.Application.Commands.Handlers;

public sealed class CreateCustomerOrderCommandHandler : IRequestHandler<CreateCustomerOrderCommand, Guid>
{
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;

    public CreateCustomerOrderCommandHandler(IOrderRepository orders, IUnitOfWork uow)
    {
        _orders = orders;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateCustomerOrderCommand request, CancellationToken cancellationToken)
    {
        if (!await _orders.DepotExistsAsync(request.PickupDepotId, cancellationToken))
            throw new KeyNotFoundException($"Depot '{request.PickupDepotId}' was not found.");

        var depotCoords = await _orders.GetDepotCoordinatesAsync(request.PickupDepotId, cancellationToken);
        if (depotCoords is null)
            throw new InvalidOperationException("Depot coordinates are missing.");

        var price = OrderPriceEstimator.Estimate(
            depotCoords.Value.Latitude,
            depotCoords.Value.Longitude,
            request.DeliveryLatitude,
            request.DeliveryLongitude,
            request.Weight,
            request.Volume,
            request.IsUrgent);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            PickupDepotId = request.PickupDepotId,
            DeliveryLatitude = request.DeliveryLatitude,
            DeliveryLongitude = request.DeliveryLongitude,
            DeliveryAddress = request.DeliveryAddress.Trim(),
            ProductDescription = request.ProductDescription.Trim(),
            Weight = request.Weight,
            Volume = request.Volume,
            Priority = request.IsUrgent ? 1 : 5,
            Status = OrderStatus.PendingAssignment,
            PriceEstimate = price,
            CreatedAt = DateTimeOffset.UtcNow,
            RequiredDeliveryBefore = request.RequiredDeliveryBefore
        };

        await _orders.AddAsync(order, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
