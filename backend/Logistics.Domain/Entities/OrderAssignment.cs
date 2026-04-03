namespace Logistics.Domain.Entities;

public sealed class OrderAssignment
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }
    public Guid RouteId { get; set; }

    public DateTimeOffset AssignedAt { get; set; }
}

