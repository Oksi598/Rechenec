using Logistics.Application.Commands;
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
        return Ok();
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
}

