using Logistics.Application.Dtos;
using Logistics.Application.Ports;
using Logistics.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Controllers;

[ApiController]
[Route("api/tms")]
public sealed class TmsQueriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TmsQueriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("drivers/{driverId:guid}/public")]
    public Task<DriverPublicDto> GetDriverPublic(
        Guid driverId,
        [FromQuery] string? scope,
        CancellationToken ct)
    {
        var maskingScope = TryParseScope(scope) ?? MaskingScope.External;

        return _mediator.Send(new GetDriverPublicDtoQuery(driverId, maskingScope), ct);
    }

    [HttpGet("contacts/{externalContactId:guid}/public")]
    public Task<ExternalContactDto> GetExternalContactPublic(
        Guid externalContactId,
        [FromQuery] string? scope,
        CancellationToken ct)
    {
        var maskingScope = TryParseScope(scope) ?? MaskingScope.External;

        return _mediator.Send(
            new GetExternalContactDtoQuery(externalContactId, maskingScope),
            ct);
    }

    private static MaskingScope? TryParseScope(string? scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
            return null;

        return Enum.TryParse<MaskingScope>(scope, ignoreCase: true, out var parsed)
            ? parsed
            : null;
    }
}

