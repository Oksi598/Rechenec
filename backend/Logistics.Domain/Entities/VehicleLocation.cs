using NetTopologySuite.Geometries;

namespace Logistics.Domain.Entities;

public sealed class VehicleLocation
{
    public Guid Id { get; set; }

    public Guid VehicleId { get; set; }
    public Point Location { get; set; } = null!;

    public decimal Speed { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
}

