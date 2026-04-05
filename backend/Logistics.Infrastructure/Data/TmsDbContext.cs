using Logistics.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Data;

public sealed class TmsDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public TmsDbContext(DbContextOptions<TmsDbContext> options)
        : base(options)
    {
    }

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

        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.Property(x => x.FullName).HasMaxLength(256);
        });

        modelBuilder.Entity<Depot>(b =>
        {
            b.Property(x => x.Latitude).HasColumnType("float");
            b.Property(x => x.Longitude).HasColumnType("float");
            b.Property(x => x.CapacityWeight).HasColumnType("float");
            b.Property(x => x.CapacityVolume).HasColumnType("float");
        });

        modelBuilder.Entity<Vehicle>(b =>
        {
            b.Property(x => x.CapacityWeight).HasColumnType("float");
            b.Property(x => x.CapacityVolume).HasColumnType("float");
            b.Property(x => x.FuelConsumption).HasColumnType("float");
        });

        modelBuilder.Entity<Order>(b =>
        {
            b.Property(x => x.DeliveryLatitude).HasColumnType("float");
            b.Property(x => x.DeliveryLongitude).HasColumnType("float");
            b.Property(x => x.Weight).HasColumnType("float");
            b.Property(x => x.Volume).HasColumnType("float");
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(128);
            b.Property(x => x.PriceEstimate).HasPrecision(18, 2);
            b.Property(x => x.DeliveryAddress).HasMaxLength(512);
            b.Property(x => x.ProductDescription).HasMaxLength(1024);
        });

        modelBuilder.Entity<Route>(ConfigureRouteConcurrency);
        modelBuilder.Entity<Route>(b =>
        {
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(128);
        });

        modelBuilder.Entity<RoutePoint>(b =>
        {
            b.Property(x => x.Latitude).HasColumnType("float");
            b.Property(x => x.Longitude).HasColumnType("float");
            b.Property(x => x.Type).HasConversion<string>().HasMaxLength(128);
        });

        modelBuilder.Entity<VehicleLocation>(b =>
        {
            b.Property(x => x.Latitude).HasColumnType("float");
            b.Property(x => x.Longitude).HasColumnType("float");
            b.Property(x => x.Speed).HasColumnType("float");
        });

        modelBuilder.Entity<DeliveryProof>(b =>
        {
            b.Property(x => x.ClientProofId).HasMaxLength(450);
            b.Property(x => x.PhotoUrl).HasMaxLength(2048);
            b.Property(x => x.Signature).HasMaxLength(2048);
            b.HasIndex(x => x.ClientProofId).IsUnique();
        });
    }

    private static void ConfigureRouteConcurrency(EntityTypeBuilder<Route> b)
    {
        b.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}
