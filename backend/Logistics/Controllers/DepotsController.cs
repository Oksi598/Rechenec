using Logistics.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Controllers;

[ApiController]
[Route("api/depots")]
[Authorize]
public sealed class DepotsController : ControllerBase
{
    private readonly TmsDbContext _db;

    public DepotsController(TmsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DepotListItemDto>>> List(CancellationToken ct)
    {
        var items = await _db.Depots.AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DepotListItemDto(d.Id, d.Name, d.Latitude, d.Longitude))
            .ToListAsync(ct);

        return Ok(items);
    }

    public sealed record DepotListItemDto(Guid Id, string Name, double Latitude, double Longitude);
}
