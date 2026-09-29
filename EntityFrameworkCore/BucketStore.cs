using System;

using Microsoft.EntityFrameworkCore;

using Mingo.ObjectStorageService.Core.Common;
using Mingo.ObjectStorageService.Core.Entities;
using Mingo.ObjectStorageService.Core.Stores;

namespace Mingo.ObjectStorageService.EntityFrameworkCore;

public class BucketStore : StoreBase, IBucketStore
{
    private AppDbContext _db => this.Db;

    public IQueryable<BucketEntity> All => _db.Buckets;

    public BucketStore(AppDbContext db) : base(db)
    {
    }

    public async Task AddAsync(BucketEntity bucket, CancellationToken cancellationToken)
    {
        await _db.AddAsync(bucket, cancellationToken);
    }

    public async Task DeleteAsync(string bucket, CancellationToken cancellationToken)
    {
        var current = await _db.Buckets.FindAsync([bucket], cancellationToken);
        if (current is null) { return; }

        _db.Remove(current);
    }

    public async Task<BucketEntity?> GetByIdAsync(string bucket, CancellationToken cancellationToken)
    {
        return await _db.Buckets.FindAsync([bucket], cancellationToken);
    }

    public async Task<PageResult<BucketEntity>> GetPagedListAsync(int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.Buckets.OrderByDescending(b => b.CreatedTime);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PageResult<BucketEntity>(items, total);
    }
}
