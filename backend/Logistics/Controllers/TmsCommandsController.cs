using Logistics.Application.Commands;
using Logistics.Application.Ports;
using Logistics.Api.Observability;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Controllers;

[ApiController]
[Route("api/tms")]
public sealed class TmsCommandsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TmsCommandsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("routes/{routeId:guid}/optimize-load")]
    public async Task<IActionResult> OptimizeLoad(Guid routeId, CancellationToken ct)
    {
        await _mediator.Send(new OptimizeLoadCommand(routeId), ct);
        TmsMetrics.TrackOptimizeLoad();
        return Ok();
    }

    [HttpPut("routes/{routeId:guid}/assignment")]
    public async Task<IActionResult> UpdateRouteAssignment(
        Guid routeId,
        [FromBody] UpdateRouteAssignmentRequest request,
        CancellationToken ct)
    {
        byte[] rowVersion;
        try
        {
            rowVersion = Convert.FromBase64String(request.RowVersion);
        }
        catch (FormatException)
        {
            return BadRequest("rowVersion must be a valid base64 string.");
        }

        try
        {
            var result = await _mediator.Send(
                new UpdateRouteAssignmentCommand(
                    routeId,
                    request.VehicleId,
                    request.DriverId,
                    rowVersion),
                ct);

            TmsMetrics.TrackRouteAssignmentUpdate();
            return Ok(new UpdateRouteAssignmentResponse(
                result.RouteId,
                result.VehicleId,
                result.DriverId,
                Convert.ToBase64String(result.RowVersion)));
        }
        catch (RouteConcurrencyException)
        {
            return Conflict(new
            {
                message = "Route assignment was updated by another process. Reload and retry."
            });
        }
        catch (VehicleAlreadyAssignedException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("delivery-proofs/sync")]
    public async Task<IActionResult> SyncDeliveryProof([FromForm] SyncDeliveryProofForm form, CancellationToken ct)
    {
        if (form.Photo is null)
            return BadRequest("Missing required form field: photo");

        if (string.IsNullOrWhiteSpace(form.ClientProofId))
            return BadRequest("Missing required form field: clientProofId");

        await using var stream = form.Photo.OpenReadStream();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, ct);

        var bytes = ms.ToArray();

        var created = await _mediator.Send(
            new SyncDeliveryProofsCommand(
                form.OrderId,
                form.ClientProofId,
                form.Signature,
                bytes,
                form.Photo.FileName),
            ct);

        TmsMetrics.TrackDeliveryProofSync();
        return Ok(new { created });
    }

    public sealed class SyncDeliveryProofForm
    {
        [FromForm(Name = "orderId")]
        public Guid OrderId { get; set; }

        [FromForm(Name = "clientProofId")]
        public string ClientProofId { get; set; } = string.Empty;

        [FromForm(Name = "signature")]
        public string Signature { get; set; } = string.Empty;

        [FromForm(Name = "photo")]
        public IFormFile? Photo { get; set; }
    }

    public sealed record UpdateRouteAssignmentRequest(
        Guid VehicleId,
        Guid DriverId,
        string RowVersion);

    public sealed record UpdateRouteAssignmentResponse(
        Guid RouteId,
        Guid VehicleId,
        Guid DriverId,
        string RowVersion);
}

