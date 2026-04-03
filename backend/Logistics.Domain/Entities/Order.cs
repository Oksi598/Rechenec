using Logistics.Domain.Enums;
using NetTopologySuite.Geometries;

namespace Logistics.Domain.Entities;

public sealed class Order
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }
    public Guid PickupDepotId { get; set; }

    public Point DeliveryLocation { get; set; } = null!;

    public decimal Weight { get; set; }
    public decimal Volume { get; set; }

    public int Priority { get; set; }
    public OrderStatus Status { get; set; }

    public decimal PriceEstimate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

