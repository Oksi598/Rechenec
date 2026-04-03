using System.Diagnostics.Metrics;

namespace Logistics.Api.Observability;

public static class TmsMetrics
{
    private static readonly Meter Meter = new("Logistics.Tms", "1.0.0");

    private static readonly Counter<long> OptimizeLoadRuns =
        Meter.CreateCounter<long>("tms.optimize_load.runs");

    private static readonly Counter<long> DeliveryProofSyncAttempts =
        Meter.CreateCounter<long>("tms.delivery_proof.sync_attempts");

    private static readonly Counter<long> TrackingUpdates =
        Meter.CreateCounter<long>("tms.tracking.location_updates");

    private static readonly Counter<long> RouteAssignmentUpdates =
        Meter.CreateCounter<long>("tms.route.assignment_updates");

    public static void TrackOptimizeLoad() => OptimizeLoadRuns.Add(1);
    public static void TrackDeliveryProofSync() => DeliveryProofSyncAttempts.Add(1);
    public static void TrackTrackingUpdate() => TrackingUpdates.Add(1);
    public static void TrackRouteAssignmentUpdate() => RouteAssignmentUpdates.Add(1);
}
