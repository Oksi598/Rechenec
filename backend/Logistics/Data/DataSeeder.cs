using Logistics.Domain;
using Logistics.Domain.Entities;
using Logistics.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Data;

public static class DataSeeder
{
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

        var config = provider.GetRequiredService<IConfiguration>();
        var dispatcherEmail = config["Seed:DispatcherEmail"] ?? "dispatcher@local.test";
        var dispatcherPassword = config["Seed:DispatcherPassword"] ?? "ChangeMe!1";

        var existing = await userManager.FindByEmailAsync(dispatcherEmail);
        if (existing is null)
        {
            var dispatcher = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = dispatcherEmail,
                Email = dispatcherEmail,
                EmailConfirmed = true,
                FullName = "System Dispatcher",
                PhoneNumber = "+380000000000",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var create = await userManager.CreateAsync(dispatcher, dispatcherPassword);
            if (!create.Succeeded)
                throw new InvalidOperationException(
                    "Failed to seed dispatcher: " + string.Join("; ", create.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(dispatcher, AppRoles.Dispatcher);
        }
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
