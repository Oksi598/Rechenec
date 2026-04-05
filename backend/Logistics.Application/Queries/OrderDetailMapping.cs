using Logistics.Application.Dtos;
using Logistics.Application.Ports;

namespace Logistics.Application.Queries;

internal static class OrderDetailMapping
{
    public static OrderDetailDto ToDto(OrderAccessData x)
    {
        var o = x.Order;
        return new OrderDetailDto(
            o.Id,
            o.Status,
            o.PriceEstimate,
            o.ProductDescription,
            o.Weight,
            o.Volume,
            o.DeliveryAddress,
            o.DeliveryLatitude,
            o.DeliveryLongitude,
            x.PickupDepotName,
            o.CreatedAt,
            o.RequiredDeliveryBefore,
            o.Priority <= 1,
            x.RouteId,
            x.RouteStart,
            x.RouteEnd);
    }
}
