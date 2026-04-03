using Logistics.Application.Dtos;
using Logistics.Application.Ports;
using Logistics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.ReadModels;

public sealed class ExternalPublicReadModelFactoryRaw : IExternalPublicReadModelFactoryRaw
{
    private readonly TmsDbContext _db;

    public ExternalPublicReadModelFactoryRaw(TmsDbContext db)
    {
        _db = db;
    }

    public async Task<ExternalContactDto> CreateAsync(Guid externalContactId, CancellationToken ct)
    {
        var user = await _db.Users
            .SingleAsync(u => u.Id == externalContactId, ct);

        // RAW: values are not masked.
        var contact = new MaskedContactDto(user.Email, user.Phone);

        return new ExternalContactDto(user.Id, user.FullName, contact);
    }
}

