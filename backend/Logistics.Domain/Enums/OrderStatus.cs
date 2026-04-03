namespace Logistics.Domain.Enums;

public enum OrderStatus
{
    PendingAssignment = 0,
    PendingLoadOptimization = 1,
    ReadyForRouting = 2,
    InRouting = 3,
    Delivered = 4,
    Cancelled = 5
}

