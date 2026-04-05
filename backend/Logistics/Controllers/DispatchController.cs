using Logistics.Domain;
using Logistics.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Controllers;

/// <summary>
/// Довідники для панелі диспетчера: маршрути, ТЗ, водії (облікові записи з роллю Driver).
/// </summary>
[ApiController]
[Route("api/dispatch")]
[Authorize(Roles = AppRoles.Dispatcher)]
public sealed class DispatchController : ControllerBase
{
    private readonly TmsDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DispatchController(TmsDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet("routes")]
    public async Task<ActionResult<IReadOnlyList<RouteListItemDto>>> Routes(CancellationToken ct)
    {
        var raw = await _db.Routes.AsNoTracking()
            .OrderByDescending(r => r.StartTime)
            .ToListAsync(ct);

        var rows = raw.Select(r => new RouteListItemDto(
                r.Id,
                r.VehicleId,
                r.DriverId,
                r.StartTime,
                r.EndTime,
                r.Status.ToString(),
                Convert.ToBase64String(r.RowVersion)))
            .ToList();

        return Ok(rows);
    }

    [HttpGet("vehicles")]
    public async Task<ActionResult<IReadOnlyList<VehicleListItemDto>>> Vehicles(CancellationToken ct)
    {
        var rows = await _db.Vehicles.AsNoTracking()
            .Where(v => v.IsActive)
            .OrderBy(v => v.PlateNumber)
            .Select(v => new VehicleListItemDto(
                v.Id,
                v.PlateNumber,
                v.VehicleType,
                v.CapacityWeight,
                v.CapacityVolume))
            .ToListAsync(ct);

        return Ok(rows);
    }

    [HttpGet("drivers")]
    public async Task<ActionResult<IReadOnlyList<DriverListItemDto>>> Drivers(CancellationToken ct)
    {
        var users = await _userManager.GetUsersInRoleAsync(AppRoles.Driver);
        var list = users
            .OrderBy(u => u.FullName)
            .Select(u => new DriverListItemDto(u.Id, u.Email ?? string.Empty, u.FullName))
            .ToList();

        return Ok(list);
    }

    public sealed record RouteListItemDto(
        Guid Id,
        Guid VehicleId,
        Guid DriverId,
        DateTimeOffset StartTime,
        DateTimeOffset EndTime,
        string Status,
        string RowVersionBase64);

    public sealed record VehicleListItemDto(
        Guid Id,
        string PlateNumber,
        string VehicleType,
        double CapacityWeight,
        double CapacityVolume);

    public sealed record DriverListItemDto(Guid Id, string Email, string FullName);
}
