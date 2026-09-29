using System;

using Microsoft.EntityFrameworkCore;

using Mingo.ObjectStorageService.Core.Common;
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

    public async Task<PageResult<ObjectEntity>> GetPagedListAsync(string bucket, int pageIndex, int pageSize, string? prefix, CancellationToken cancellationToken)
    {
        var query = _db.Objects.Where(o => o.BucketName == bucket);
        if (!string.IsNullOrEmpty(prefix))
        {
            query = query.Where(o => o.Id.StartsWith(prefix));
        }

        var ordered = query.OrderByDescending(o => o.CreatedTime);
        var total = await ordered.CountAsync(cancellationToken);
        var items = await ordered.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PageResult<ObjectEntity>(items, total);
    }

    public IQueryable<ObjectEntity> All => _db.Set<ObjectEntity>();
}
