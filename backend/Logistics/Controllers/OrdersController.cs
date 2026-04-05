using Logistics.Application.Commands;
using Logistics.Application.Dtos;
using Logistics.Application.Queries;
using Logistics.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Logistics.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = AppRoles.Customer)]
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateOrderApiRequest body, CancellationToken ct)
    {
        var userId = RequireUserId();

        var id = await _mediator.Send(
            new CreateCustomerOrderCommand(
                userId,
                body.PickupDepotId,
                body.DeliveryLatitude,
                body.DeliveryLongitude,
                body.DeliveryAddress?.Trim() ?? string.Empty,
                body.ProductDescription?.Trim() ?? string.Empty,
                body.Weight,
                body.Volume,
                body.IsUrgent,
                body.RequiredDeliveryBefore),
            ct);

        return Ok(id);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderDetailDto>> GetById(Guid orderId, CancellationToken ct)
    {
        var dto = await _mediator.Send(new GetOrderForCurrentUserQuery(orderId), ct);
        if (dto is null)
            return NotFound();

        return Ok(dto);
    }

    [Authorize(Roles = AppRoles.Customer)]
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<OrderDetailDto>>> MyOrders(CancellationToken ct)
    {
        var list = await _mediator.Send(new ListMyCustomerOrdersQuery(), ct);
        return Ok(list);
    }

    [Authorize(Roles = AppRoles.Driver)]
    [HttpGet("driver")]
    public async Task<ActionResult<IReadOnlyList<OrderDetailDto>>> DriverOrders(CancellationToken ct)
    {
        var list = await _mediator.Send(new ListDriverOrdersQuery(), ct);
        return Ok(list);
    }

    [Authorize(Roles = AppRoles.Warehouse)]
    [HttpGet("warehouse-board")]
    public async Task<ActionResult<IReadOnlyList<OrderDetailDto>>> WarehouseBoard(CancellationToken ct)
    {
        var list = await _mediator.Send(new ListWarehouseOrdersQuery(), ct);
        return Ok(list);
    }

    [Authorize(Roles = AppRoles.Dispatcher)]
    [HttpGet("all")]
    public async Task<ActionResult<IReadOnlyList<OrderDetailDto>>> AllOrders(CancellationToken ct)
    {
        var list = await _mediator.Send(new ListDispatcherOrdersQuery(), ct);
        return Ok(list);
    }

    private Guid RequireUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(id, out var userId))
            throw new UnauthorizedAccessException();

        return userId;
    }

    public sealed record CreateOrderApiRequest(
        Guid PickupDepotId,
        double DeliveryLatitude,
        double DeliveryLongitude,
        string DeliveryAddress,
        string ProductDescription,
        double Weight,
        double Volume,
        bool IsUrgent,
        DateTimeOffset? RequiredDeliveryBefore);
}
