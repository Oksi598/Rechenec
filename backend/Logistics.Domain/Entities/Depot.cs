using NetTopologySuite.Geometries;

namespace Logistics.Domain.Entities;

public sealed class Depot
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Geography point in SRID=4326. NTS uses (X=Lon, Y=Lat).
    /// </summary>
    public Point Location { get; set; } = null!;

    public decimal CapacityWeight { get; set; }
    public decimal CapacityVolume { get; set; }
}

