using System;

using Mingo.ObjectStorageService.Core.Common;
using Mingo.ObjectStorageService.Core.Entities;

namespace Mingo.ObjectStorageService.Core.Stores;

public interface IBucketQuery
{
    IQueryable<BucketEntity> All { get; }
}

public interface IBucketStore : IStoreBase, IBucketQuery
{
    Task AddAsync(BucketEntity bucket, CancellationToken cancellationToken);
    Task DeleteAsync(string bucket, CancellationToken cancellationToken);
    Task<BucketEntity?> GetByIdAsync(string bucket, CancellationToken cancellationToken);
    Task<PageResult<BucketEntity>> GetPagedListAsync(int pageIndex, int pageSize, CancellationToken cancellationToken);
}
