using System;

using Microsoft.Extensions.Logging;

using Mingo.ObjectStorageService.Core.Entities;
using Mingo.ObjectStorageService.Core.Stores;

namespace Mingo.ObjectStorageService.Core.Services;

public interface IBucketService
{
    Task CreateAsync(string bucket, CancellationToken cancellationToken);
    Task DeleteAsync(string bucket, CancellationToken cancellationToken);
}

public class BucketService : IBucketService
{
    private readonly IStorageProvider _storageProvider;
    private readonly ILogger _logger;
    private readonly IBucketStore _store;

    public BucketService(IStorageProvider storageProvider, ILogger<BucketService> logger, IBucketStore store)
    {
        _logger = logger;
        _storageProvider = storageProvider;
        _store = store;
    }

    public async Task CreateAsync(string bucket, CancellationToken cancellationToken)
    {
        var entity = await _store.GetByIdAsync(bucket, cancellationToken);
        if (entity is null)
        {
            entity = new BucketEntity(bucket);
            await _store.AddAsync(entity, cancellationToken);
        }

        await _store.SaveChangesAsync(cancellationToken);
        await _storageProvider.EnsureBucketCreatedAsync(entity.Id, cancellationToken);
    }

    public async Task DeleteAsync(string bucket, CancellationToken cancellationToken)
    {
        var entity = await _store.GetByIdAsync(bucket, cancellationToken);
        if (entity is null) { return; }

        var isEmpty = await _storageProvider.EnsureBucketIsEmptyAsync(entity.Id, cancellationToken);
        if (!isEmpty)
        {
            throw new InvalidOperationException("Cannot delete not empty bucket");
        }

        await _storageProvider.EnsureBucketDeletedAsync(entity.Id, cancellationToken);
        await _store.DeleteAsync(entity.Id, cancellationToken);
        await _store.SaveChangesAsync(cancellationToken);
    }
}
