namespace Logistics.Application.Services;

public static class OrderPriceEstimator
{
    private const double EarthRadiusKm = 6371.0;

    public static decimal Estimate(
        double depotLat,
        double depotLng,
        double deliveryLat,
        double deliveryLng,
        double weightKg,
        double volumeM3,
        bool isUrgent)
    {
        var distanceKm = HaversineKm(depotLat, depotLng, deliveryLat, deliveryLng);

        var distancePart = 2.5m * (decimal)distanceKm;
        var weightPart = 0.05m * (decimal)weightKg;
        var volumePart = 8m * (decimal)volumeM3;
        var urgentPart = isUrgent ? 25m : 0m;

        var total = distancePart + weightPart + volumePart + urgentPart;
        return Math.Round(Math.Max(0m, total), 2);
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        static double ToRad(double deg) => deg * Math.PI / 180.0;

        var rLat1 = ToRad(lat1);
        var rLon1 = ToRad(lon1);
        var rLat2 = ToRad(lat2);
        var rLon2 = ToRad(lon2);

        var dLat = rLat2 - rLat1;
        var dLon = rLon2 - rLon1;

        var sinDLat = Math.Sin(dLat / 2.0);
        var sinDLon = Math.Sin(dLon / 2.0);

        var h = sinDLat * sinDLat + Math.Cos(rLat1) * Math.Cos(rLat2) * sinDLon * sinDLon;
        var c = 2.0 * Math.Asin(Math.Min(1.0, Math.Sqrt(h)));
        return EarthRadiusKm * c;
    }
}
