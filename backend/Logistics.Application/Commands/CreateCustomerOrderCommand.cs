using MediatR;

namespace Logistics.Application.Commands;

public sealed record CreateCustomerOrderCommand(
    Guid CustomerId,
    Guid PickupDepotId,
    double DeliveryLatitude,
    double DeliveryLongitude,
    string DeliveryAddress,
    string ProductDescription,
    double Weight,
    double Volume,
    bool IsUrgent,
    DateTimeOffset? RequiredDeliveryBefore) : IRequest<Guid>;
