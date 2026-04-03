using MediatR;

namespace Logistics.Application.Commands;

public sealed record UpdateRouteAssignmentCommand(
    Guid RouteId,
    Guid VehicleId,
    Guid DriverId,
    byte[] RowVersion) : IRequest<UpdateRouteAssignmentResult>;

public sealed record UpdateRouteAssignmentResult(
    Guid RouteId,
    Guid VehicleId,
    Guid DriverId,
    byte[] RowVersion);
