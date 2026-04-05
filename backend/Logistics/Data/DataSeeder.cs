using Logistics.Domain;
using Logistics.Domain.Entities;
using Logistics.Domain.Enums;
using Logistics.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Data;

public static class DataSeeder
{
    /// <summary>Єдиний сидований диспетчер (логін через /api/auth/login).</summary>
    public const string DispatcherEmail = "dispatcher@local.test";

    public const string DispatcherPassword = "ChangeMe!1";

    /// <summary>Демо-водій для перевірки UI (той самий пароль).</summary>
    public const string DemoDriverEmail = "driver@local.test";

    public const string DemoDriverPassword = "ChangeMe!1";

    /// <summary>Демо-працівник складу (той самий пароль).</summary>
    public const string DemoWarehouseEmail = "warehouse@local.test";

    public const string DemoWarehousePassword = "ChangeMe!1";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = provider.GetRequiredService<TmsDbContext>();

        foreach (var role in new[]
                 {
                     AppRoles.Dispatcher, AppRoles.Driver, AppRoles.Customer, AppRoles.Warehouse
                 })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = role,
                    NormalizedName = role.ToUpperInvariant()
                });
            }
        }

        await SeedDepotsIfEmptyAsync(db, ct);

        var existing = await userManager.FindByEmailAsync(DispatcherEmail);
        if (existing is null)
        {
            var dispatcher = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = DispatcherEmail,
                Email = DispatcherEmail,
                EmailConfirmed = true,
                FullName = "System Dispatcher",
                PhoneNumber = "+380000000000",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var create = await userManager.CreateAsync(dispatcher, DispatcherPassword);
            if (!create.Succeeded)
                throw new InvalidOperationException(
                    "Failed to seed dispatcher: " + string.Join("; ", create.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(dispatcher, AppRoles.Dispatcher);
        }

        await SeedDemoDriverVehicleRouteAsync(db, userManager, ct);
        await SeedDemoWarehouseUserAsync(userManager, ct);
    }

    private static async Task SeedDemoWarehouseUserAsync(UserManager<ApplicationUser> userManager, CancellationToken ct)
    {
        var existing = await userManager.FindByEmailAsync(DemoWarehouseEmail);
        if (existing is not null)
            return;

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = DemoWarehouseEmail,
            Email = DemoWarehouseEmail,
            EmailConfirmed = true,
            FullName = "Demo Warehouse",
            PhoneNumber = "+380222222222",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var created = await userManager.CreateAsync(user, DemoWarehousePassword);
        if (!created.Succeeded)
            return;

        await userManager.AddToRoleAsync(user, AppRoles.Warehouse);
    }

    private static async Task SeedDemoDriverVehicleRouteAsync(
        TmsDbContext db,
        UserManager<ApplicationUser> userManager,
        CancellationToken ct)
    {
        var demoDriver = await userManager.FindByEmailAsync(DemoDriverEmail);
        if (demoDriver is null)
        {
            demoDriver = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = DemoDriverEmail,
                Email = DemoDriverEmail,
                EmailConfirmed = true,
                FullName = "Demo Driver",
                PhoneNumber = "+380111111111",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var created = await userManager.CreateAsync(demoDriver, DemoDriverPassword);
            if (!created.Succeeded)
                return;

            await userManager.AddToRoleAsync(demoDriver, AppRoles.Driver);
        }

        if (!await db.Vehicles.AnyAsync(ct))
        {
            db.Vehicles.Add(new Vehicle
            {
                Id = Guid.NewGuid(),
                PlateNumber = "DEMO-01",
                CapacityWeight = 3_500,
                CapacityVolume = 18,
                FuelConsumption = 12,
                VehicleType = "van",
                IsActive = true
            });
            await db.SaveChangesAsync(ct);
        }

        if (await db.Routes.AnyAsync(ct))
            return;

        var vehicle = await db.Vehicles.AsNoTracking().FirstAsync(ct);
        var route = new Logistics.Domain.Entities.Route
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            DriverId = demoDriver.Id,
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(10),
            Status = RouteStatus.Planned
        };
        db.Routes.Add(route);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedDepotsIfEmptyAsync(TmsDbContext db, CancellationToken ct)
    {
        if (await db.Depots.AnyAsync(ct))
            return;

        db.Depots.AddRange(
            new Depot
            {
                Id = Guid.NewGuid(),
                Name = "Depot North",
                Latitude = 50.4501,
                Longitude = 30.5234,
                CapacityWeight = 50_000,
                CapacityVolume = 500
            },
            new Depot
            {
                Id = Guid.NewGuid(),
                Name = "Depot South",
                Latitude = 49.8397,
                Longitude = 24.0297,
                CapacityWeight = 40_000,
                CapacityVolume = 400
            });

        await db.SaveChangesAsync(ct);
    }
}
