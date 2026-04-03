using Logistics.Application.Commands;
using Logistics.Application.Ports;
using MediatR;

namespace Logistics.Application.Commands.Handlers;

public sealed class UpdateRouteAssignmentCommandHandler
    : IRequestHandler<UpdateRouteAssignmentCommand, UpdateRouteAssignmentResult>
{
    private readonly IRouteAssignmentService _assignmentService;

    public UpdateRouteAssignmentCommandHandler(IRouteAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    public async Task<UpdateRouteAssignmentResult> Handle(
        UpdateRouteAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var snapshot = await _assignmentService.UpdateAssignmentAsync(
            request.RouteId,
            request.VehicleId,
            request.DriverId,
            request.RowVersion,
            cancellationToken);

        return new UpdateRouteAssignmentResult(
            snapshot.RouteId,
            snapshot.VehicleId,
            snapshot.DriverId,
            snapshot.RowVersion);
    }
}
