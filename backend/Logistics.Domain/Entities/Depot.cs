namespace Logistics.Domain.Entities;

public sealed class Depot
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public double CapacityWeight { get; set; }
    public double CapacityVolume { get; set; }
}
