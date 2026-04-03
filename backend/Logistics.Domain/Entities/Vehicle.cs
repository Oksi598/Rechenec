namespace Logistics.Domain.Entities;

public sealed class Vehicle
{
    public Guid Id { get; set; }

    public string PlateNumber { get; set; } = string.Empty;

    public decimal CapacityWeight { get; set; }
    public decimal CapacityVolume { get; set; }

    public decimal FuelConsumption { get; set; }
    public string VehicleType { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

