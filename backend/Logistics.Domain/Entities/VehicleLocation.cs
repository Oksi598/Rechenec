namespace Logistics.Domain.Entities;

public sealed class VehicleLocation
{
    public Guid Id { get; set; }

    public Guid VehicleId { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public double Speed { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
}
