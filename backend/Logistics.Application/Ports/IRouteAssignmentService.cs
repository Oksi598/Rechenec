namespace Logistics.Application.Ports;

public interface IRouteAssignmentService
{
    Task<RouteAssignmentSnapshot> UpdateAssignmentAsync(
        Guid routeId,
        Guid vehicleId,
        Guid driverId,
        byte[] expectedRowVersion,
        CancellationToken ct);
}

public sealed record RouteAssignmentSnapshot(
    Guid RouteId,
    Guid VehicleId,
    Guid DriverId,
    byte[] RowVersion);

public sealed class RouteConcurrencyException : Exception
{
    public RouteConcurrencyException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}

public sealed class VehicleAlreadyAssignedException : Exception
{
    public VehicleAlreadyAssignedException(Guid vehicleId)
        : base($"Vehicle '{vehicleId}' is already assigned to another active route.")
    {
    }
}
