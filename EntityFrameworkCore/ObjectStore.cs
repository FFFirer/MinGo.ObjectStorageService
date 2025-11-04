using System;

using Mingo.ObjectStorageService.Core.Entities;
using Mingo.ObjectStorageService.Core.Stores;

namespace Mingo.ObjectStorageService.EntityFrameworkCore;

public class ObjectStore : StoreBase, IObjectStore
{
    private AppDbContext _db => this.Db;
    public ObjectStore(AppDbContext db) : base(db)
    {
    }

    public async Task AddAsync(ObjectEntity objectEntity, CancellationToken cancellationToken)
    {
        await _db.AddAsync(objectEntity, cancellationToken);
    }

    public async Task DeleteAsync(string bucket, string id, CancellationToken cancellationToken)
    {
        var entity = await _db.Objects.FindAsync([bucket, id], cancellationToken);
        if (entity is null) { return; }

        _db.Remove(entity);
    }

    public async Task<ObjectEntity?> GetAsync(string bucket, string id, CancellationToken cancellationToken)
    {
        return await _db.Objects.FindAsync([bucket, id], cancellationToken);
    }

    public IQueryable<ObjectEntity> All => _db.Set<ObjectEntity>();
}
