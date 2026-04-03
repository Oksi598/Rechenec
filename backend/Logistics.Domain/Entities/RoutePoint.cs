using Logistics.Domain.Enums;

namespace Logistics.Domain.Entities;

public sealed class RoutePoint
{
    public Guid Id { get; set; }

    public Guid RouteId { get; set; }
    public Guid OrderId { get; set; }

    public int Sequence { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public DateTimeOffset? ArrivalTime { get; set; }
    public DateTimeOffset? DepartureTime { get; set; }

    public RoutePointType Type { get; set; }
}
