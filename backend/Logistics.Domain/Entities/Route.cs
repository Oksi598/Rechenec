using Logistics.Domain.Enums;

namespace Logistics.Domain.Entities;

public sealed class Route
{
    public Guid Id { get; set; }

    public Guid VehicleId { get; set; }
    public Guid DriverId { get; set; }

    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }

    public RouteStatus Status { get; set; }

    /// <summary>
    /// Optimistic concurrency token to avoid double assignment.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

