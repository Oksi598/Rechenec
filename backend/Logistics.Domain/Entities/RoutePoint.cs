using Logistics.Domain.Enums;
using NetTopologySuite.Geometries;

namespace Logistics.Domain.Entities;

public sealed class RoutePoint
{
    public Guid Id { get; set; }

    public Guid RouteId { get; set; }
    public Guid OrderId { get; set; }

    public int Sequence { get; set; }

    public Point Location { get; set; } = null!;

    public DateTimeOffset? ArrivalTime { get; set; }
    public DateTimeOffset? DepartureTime { get; set; }

    public RoutePointType Type { get; set; }
}

