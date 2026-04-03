using Logistics.Domain.Enums;

namespace Logistics.Domain.Entities;

public sealed class Order
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }
    public Guid PickupDepotId { get; set; }

    public double DeliveryLatitude { get; set; }
    public double DeliveryLongitude { get; set; }

    public double Weight { get; set; }
    public double Volume { get; set; }

    public int Priority { get; set; }
    public OrderStatus Status { get; set; }

    public decimal PriceEstimate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
