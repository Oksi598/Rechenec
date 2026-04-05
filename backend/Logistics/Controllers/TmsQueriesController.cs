using Logistics.Application.Dtos;
using Logistics.Application.Ports;
using Logistics.Application.Queries;
using Logistics.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Logistics.Api.Controllers;

[ApiController]
[Route("api/tms")]
[Authorize]
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
        CancellationToken ct)
    {
        var maskingScope = ResolveMaskingScope(User);

        return _mediator.Send(new GetDriverPublicDtoQuery(driverId, maskingScope), ct);
    }

    [HttpGet("contacts/{externalContactId:guid}/public")]
    public Task<ExternalContactDto> GetExternalContactPublic(
        Guid externalContactId,
        CancellationToken ct)
    {
        var maskingScope = ResolveMaskingScope(User);

        return _mediator.Send(
            new GetExternalContactDtoQuery(externalContactId, maskingScope),
            ct);
    }

    private static MaskingScope ResolveMaskingScope(ClaimsPrincipal user)
    {
        return user.IsInRole(AppRoles.Dispatcher) || user.IsInRole(AppRoles.Warehouse)
            ? MaskingScope.Internal
            : MaskingScope.External;
    }
}

