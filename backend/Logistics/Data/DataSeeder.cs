using Logistics.Domain;
using Logistics.Domain.Entities;
using Logistics.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Data;

public static class DataSeeder
{
    /// <summary>Єдиний сидований диспетчер (логін через /api/auth/login).</summary>
    public const string DispatcherEmail = "dispatcher@local.test";

    public const string DispatcherPassword = "ChangeMe!1";

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
