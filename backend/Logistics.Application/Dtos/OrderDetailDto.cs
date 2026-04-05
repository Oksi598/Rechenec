using Logistics.Domain.Enums;

namespace Logistics.Application.Dtos;

public sealed record OrderDetailDto(
    Guid Id,
    OrderStatus Status,
    decimal PriceEstimate,
    string ProductDescription,
    double Weight,
    double Volume,
    string DeliveryAddress,
    double DeliveryLatitude,
    double DeliveryLongitude,
    string PickupDepotName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? RequiredDeliveryBefore,
    bool IsUrgent,
    Guid? RouteId,
    DateTimeOffset? RouteStartTime,
    DateTimeOffset? RouteEndTime);
