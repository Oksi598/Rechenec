using Logistics.Application.Dtos;
using Logistics.Application.Ports;
using Logistics.Domain.Entities;
using Logistics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.ReadModels;

public sealed class DriverPublicReadModelFactoryRaw : IDriverPublicReadModelFactoryRaw
{
    private readonly TmsDbContext _db;

    public DriverPublicReadModelFactoryRaw(TmsDbContext db)
    {
        _db = db;
    }

    public async Task<DriverPublicDto> CreateAsync(Guid driverId, CancellationToken ct)
    {
        var user = await _db.Users
            .SingleAsync(u => u.Id == driverId, ct);

        // This is RAW: values are not masked.
        var contact = new MaskedContactDto(user.Email, user.Phone);

        return new DriverPublicDto(user.Id, user.FullName, contact);
    }
}

