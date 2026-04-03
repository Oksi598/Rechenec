using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

namespace Logistics.Infrastructure.Data;

public sealed class TmsDbContext : DbContext
{
    public TmsDbContext(DbContextOptions<TmsDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Depot> Depots => Set<Depot>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<RoutePoint> RoutePoints => Set<RoutePoint>();
    public DbSet<VehicleLocation> VehicleLocations => Set<VehicleLocation>();
    public DbSet<OrderAssignment> OrderAssignments => Set<OrderAssignment>();
    public DbSet<DeliveryProof> DeliveryProofs => Set<DeliveryProof>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Route>(ConfigureRouteConcurrency);
        ConfigureGeographyPoint(modelBuilder);

        // Enums -> int by convention is enough; specify explicitly for clarity.
        modelBuilder.Entity<Order>().Property(x => x.Status).HasConversion<int>();
        modelBuilder.Entity<Route>().Property(x => x.Status).HasConversion<int>();
        modelBuilder.Entity<RoutePoint>().Property(x => x.Type).HasConversion<int>();

        modelBuilder.Entity<DeliveryProof>()
            .HasIndex(x => x.ClientProofId)
            .IsUnique();
    }

    private static void ConfigureRouteConcurrency(EntityTypeBuilder<Route> b)
    {
        b.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
    }

    /// <summary>
    /// Sets NTS points as SQL Server geography with SRID=4326.
    /// </summary>
    private static void ConfigureGeographyPoint(ModelBuilder modelBuilder)
    {
        // Note: SRID must be consistent across geometry computations (Haversine and spatial queries).
        // Some EF Core provider versions don't expose HasSrid() fluent API; we enforce SRID on Point values.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var prop in entityType.GetProperties())
            {
                if (prop.ClrType == typeof(Point))
                {
                    modelBuilder.Entity(entityType.ClrType!)
                        .Property<Point>(prop.Name)
                        .HasColumnType("geography");
                }
            }
        }
    }

    // Intentionally left empty: this DbContext is extended by modules (repositories/handlers).
}

