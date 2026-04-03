namespace Logistics.Domain.Entities;

public sealed class Vehicle
{
    public Guid Id { get; set; }

    public string PlateNumber { get; set; } = string.Empty;

    public double CapacityWeight { get; set; }
    public double CapacityVolume { get; set; }

    public double FuelConsumption { get; set; }
    public string VehicleType { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
