using System;

using Microsoft.Extensions.Logging;

using Mingo.ObjectStorageService.Core.Entities;
using Mingo.ObjectStorageService.Core.Stores;

namespace Mingo.ObjectStorageService.Core.Services;

public interface IObjectService
{
    Task DeleteAsync(ObjectInfo objectInfo, CancellationToken cancellationToken);
    Task<IDownloadResponse?> GetDownloadAsync(ObjectInfo objectInfo, CancellationToken cancellationToken);
    Task SaveAsync(Stream stream, UploadObjectInfo objectInfo, CancellationToken cancellationToken);
}

public class ObjectService : IObjectService
{
    private readonly ILogger _logger;
    private readonly IStorageProvider _storageProvider;
    private readonly IObjectStore _store;

    public ObjectService(ILogger<ObjectService> logger, IStorageProvider storageProvider, IObjectStore store)
    {
        _logger = logger;
        _storageProvider = storageProvider;
        _store = store;
    }

    public async Task DeleteAsync(ObjectInfo objectInfo, CancellationToken cancellationToken)
    {
        await _store.DeleteAsync(objectInfo.Bucket, objectInfo.Id, cancellationToken);
        await _storageProvider.DeleteAsync(objectInfo.Bucket, objectInfo.Id, cancellationToken);
        await _store.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDownloadResponse?> GetDownloadAsync(ObjectInfo objectInfo, CancellationToken cancellationToken)
    {
        var objectEntity = await _store.GetAsync(objectInfo.Bucket, objectInfo.Id, cancellationToken);
        if (objectEntity is null)
        {
            return default;
        }

        // todo: check auth
        var storageInfo = await _storageProvider.GetObjectAsync(objectEntity.BucketName, objectEntity.Id, cancellationToken);
        return storageInfo switch
        {
            null => null,
            FileSystemStorageInfo fs => new DownloadFromPathResponse(fs.FullPath, true, objectEntity.FileName, objectEntity.Metadata.GetValueOrDefault(ObjectMetadataKeys.ContentType), objectEntity.LastModified),
            _ => throw new NotSupportedException()
        };
    }

    public async Task SaveAsync(Stream stream, UploadObjectInfo objectInfo, CancellationToken cancellationToken)
    {
        var objectEntity = await _store.GetAsync(objectInfo.Bucket, objectInfo.Id, cancellationToken);
        if (objectEntity is null)
        {
            objectEntity = new(objectInfo.Id, objectInfo.Bucket, objectInfo.FileName);
            objectEntity.Metadata[ObjectMetadataKeys.ContentType] = objectInfo.ContentType;
            await _store.AddAsync(objectEntity, cancellationToken);
        }

        await _storageProvider.SaveAsync(stream, new(objectInfo.Bucket, objectInfo.Id), cancellationToken);
        await _store.SaveChangesAsync(cancellationToken);
    }
}

public interface IDownloadResponse
{
    string FileName { get; }
    DateTimeOffset? LastModified { get; }
}

public record ObjectInfo(string Bucket, string Id);
public record DownloadFromPathResponse(string Path, bool IsPhysicalPath, string FileName, string? ContentType, DateTimeOffset? LastModified) : IDownloadResponse;
public record DownloadFromStreamResponse(Stream Stream, string FileName, string? ContentType, DateTimeOffset? LastModified) : IDownloadResponse;

public record UploadObjectInfo(string Bucket, string Id, string FileName, string ContentType, long Size);