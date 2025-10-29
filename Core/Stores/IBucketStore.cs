using System;

using Mingo.ObjectStorageService.Core.Entities;

namespace Mingo.ObjectStorageService.Core.Stores;

public interface IBucketQuery
{
    
}

public interface IBucketStore : IStoreBase
{
    Task AddAsync(BucketEntity bucket, CancellationToken cancellationToken);
    Task DeleteAsync(string bucket, CancellationToken cancellationToken);
    Task<BucketEntity?> GetByIdAsync(string bucket, CancellationToken cancellationToken);
   
}
