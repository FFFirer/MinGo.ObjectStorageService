using System;

using Mingo.ObjectStorageService.Core.Common;
using Mingo.ObjectStorageService.Core.Entities;

namespace Mingo.ObjectStorageService.Core.Stores;

public interface IObjectQuery
{
    IQueryable<ObjectEntity> All { get; }
}

public interface IObjectStore : IObjectQuery, IStoreBase
{
    Task AddAsync(ObjectEntity objectEntity, CancellationToken cancellationToken);
    Task DeleteAsync(string bucket, string id, CancellationToken cancellationToken);
    Task<ObjectEntity?> GetAsync(string bucket, string id, CancellationToken cancellationToken);
    Task<PageResult<ObjectEntity>> GetPagedListAsync(string bucket, int pageIndex, int pageSize, string? prefix, CancellationToken cancellationToken);
}
