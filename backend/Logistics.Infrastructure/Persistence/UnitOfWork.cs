using Logistics.Application.Ports;
using Logistics.Infrastructure.Data;

namespace Logistics.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly TmsDbContext _db;

    public UnitOfWork(TmsDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}

