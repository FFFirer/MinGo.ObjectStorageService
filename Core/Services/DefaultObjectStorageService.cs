using System;

using Microsoft.Extensions.Logging;

using Mingo.ObjectStorageService.Core.Common;
using Mingo.ObjectStorageService.Core.Entities;
using Mingo.ObjectStorageService.Core.Stores;

namespace Mingo.ObjectStorageService.Core.Services;

public interface IObjectStorageService
{
    Task CreateBucketAsync(string name, CancellationToken cancellationToken);
    Task DeleteBucketAsync(string name, CancellationToken cancellationToken);
    Task<PageResult<BucketSummary>> ListBucketsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken);

    Task DeleteObjectAsync(ObjectInfo objectInfo, CancellationToken cancellationToken);
    Task<IDownloadResponse?> GetObjectDownloadAsync(ObjectInfo objectInfo, CancellationToken cancellationToken);
    Task<ObjectMetadataResult?> GetObjectMetadataAsync(ObjectInfo objectInfo, CancellationToken cancellationToken);
    Task<PageResult<ObjectSummary>> ListObjectsAsync(string bucket, int pageIndex, int pageSize, string? prefix, CancellationToken cancellationToken);
    Task SaveObjectAsync(Stream stream, UploadObjectInfo uploadInfo, CancellationToken cancellationToken);

    Task<Stream> OpenObjectSaveStreamAsync(UploadObjectInfo uploadInfo, CancellationToken cancellationToken);
}

public class DefaultObjectStorageService : IObjectStorageService
{
    private readonly ILogger _logger;
    private readonly IObjectStore _objectStore;
    private readonly IBucketStore _bucketStore;
    private readonly IStorageProvider _storageProvider;

    public DefaultObjectStorageService(ILogger<DefaultObjectStorageService> logger, IObjectStore objectStore, IBucketStore bucketStore, IStorageProvider storageProvider)
    {
        _logger = logger;
        _objectStore = objectStore;
        _bucketStore = bucketStore;
        _storageProvider = storageProvider;
    }

    public async Task CreateBucketAsync(string name, CancellationToken cancellationToken)
    {
        var entity = await _bucketStore.GetByIdAsync(name, cancellationToken);
        if (entity is null)
        {
            entity = new BucketEntity(name);
            await _bucketStore.AddAsync(entity, cancellationToken);
        }

        await _storageProvider.EnsureBucketCreatedAsync(entity.Id, cancellationToken);
        await _bucketStore.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteBucketAsync(string name, CancellationToken cancellationToken)
    {
        var entity = await _bucketStore.GetByIdAsync(name, cancellationToken);
        if (entity is null) { return; }

        var isEmpty = await _storageProvider.EnsureBucketIsEmptyAsync(entity.Id, cancellationToken);
        if (!isEmpty)
        {
            throw new InvalidOperationException("Cannot delete not empty bucket");
        }

        await _storageProvider.EnsureBucketDeletedAsync(entity.Id, cancellationToken);
        await _bucketStore.DeleteAsync(entity.Id, cancellationToken);
        await _bucketStore.SaveChangesAsync(cancellationToken);
    }

    public async Task<PageResult<BucketSummary>> ListBucketsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        var page = await _bucketStore.GetPagedListAsync(pageIndex, pageSize, cancellationToken);
        var items = page.Datas.Select(b => new BucketSummary(b.Id, b.Guid, b.CreatedTime)).ToList();
        return new PageResult<BucketSummary>(items, page.TotalCount);
    }

    public async Task DeleteObjectAsync(ObjectInfo objectInfo, CancellationToken cancellationToken)
    {
        await _objectStore.DeleteAsync(objectInfo.Bucket, objectInfo.Id, cancellationToken);
        await _storageProvider.DeleteAsync(objectInfo.Bucket, objectInfo.Id, cancellationToken);
        await _objectStore.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDownloadResponse?> GetObjectDownloadAsync(ObjectInfo objectInfo, CancellationToken cancellationToken)
    {
        var objectEntity = await _objectStore.GetAsync(objectInfo.Bucket, objectInfo.Id, cancellationToken);
        if (objectEntity is null)
        {
            return default;
        }

        // todo: check auth
        var storageInfo = await _storageProvider.GetObjectAsync(objectEntity.BucketName, objectEntity.Id, cancellationToken);
        return storageInfo switch
        {
            null => null,
            FileSystemStorageInfo fs => new DownloadFromPathResponse(fs.FullPath, true, objectEntity.FileName, objectEntity.Metadata.GetValueOrDefault(ObjectMetadataKeys.ContentType), objectEntity.LastModified, objectEntity.Metadata),
            _ => throw new NotSupportedException()
        };
    }

    public async Task<ObjectMetadataResult?> GetObjectMetadataAsync(ObjectInfo objectInfo, CancellationToken cancellationToken)
    {
        var objectEntity = await _objectStore.GetAsync(objectInfo.Bucket, objectInfo.Id, cancellationToken);
        if (objectEntity is null) { return null; }

        return new ObjectMetadataResult(objectEntity.Id, objectEntity.BucketName, objectEntity.FileName, objectEntity.CreatedTime, objectEntity.LastModified, objectEntity.Metadata);
    }

    public async Task<PageResult<ObjectSummary>> ListObjectsAsync(string bucket, int pageIndex, int pageSize, string? prefix, CancellationToken cancellationToken)
    {
        var page = await _objectStore.GetPagedListAsync(bucket, pageIndex, pageSize, prefix, cancellationToken);
        var items = page.Datas.Select(o => new ObjectSummary(o.Id, o.BucketName, o.FileName, o.CreatedTime, o.LastModified, o.Metadata)).ToList();
        return new PageResult<ObjectSummary>(items, page.TotalCount);
    }

    public async Task<Stream> OpenObjectSaveStreamAsync(UploadObjectInfo uploadInfo, CancellationToken cancellationToken)
    {
        var (bucketEntity, objectEntity) = await SaveObjectMetadata(uploadInfo, cancellationToken);

        return _storageProvider.OpenWriteStream(new(bucketEntity.Id, objectEntity.Id), cancellationToken);
    }

    public async Task SaveObjectAsync(Stream stream, UploadObjectInfo uploadInfo, CancellationToken cancellationToken)
    {
        var (bucketEntity, objectEntity) = await SaveObjectMetadata(uploadInfo, cancellationToken);

        await _storageProvider.SaveAsync(stream, new(bucketEntity.Id, objectEntity.Id), cancellationToken);
    }
    
    private async Task<(BucketEntity, ObjectEntity)> SaveObjectMetadata(UploadObjectInfo uploadInfo, CancellationToken cancellationToken)
    {
        var bucketEntity = await _bucketStore.GetByIdAsync(uploadInfo.Bucket, cancellationToken);
        if (bucketEntity is null)
        {
            throw new InvalidOperationException($"Bucket '{uploadInfo.Bucket}' not exists");
        }

        var metadata = uploadInfo.Metadata.NormalizeObjectMetadata();
        metadata[ObjectMetadataKeys.Size] = uploadInfo.Size.ToString();

        var objectEntity = await _objectStore.GetAsync(uploadInfo.Bucket, uploadInfo.Id, cancellationToken);
        if (objectEntity is null)
        {
            var rawFileName = Path.GetFileName(uploadInfo.FileName);

            objectEntity = new(uploadInfo.Id, bucketEntity.Id, rawFileName);
            objectEntity.Metadata = metadata;

            await _objectStore.AddAsync(objectEntity, cancellationToken);
        }

        objectEntity.LastModified = DateTimeOffset.UtcNow;
        objectEntity.Metadata = metadata;
        await _objectStore.SaveChangesAsync(cancellationToken);

        return (bucketEntity, objectEntity);
    }
}


public interface IDownloadResponse
{
    string FileName { get; }
    DateTimeOffset? LastModified { get; }
    IDictionary<string, string> Metadata { get; }
}

public record ObjectInfo(string Bucket, string Id);
public record DownloadFromPathResponse(string Path, bool IsPhysicalPath, string FileName, string? ContentType, DateTimeOffset? LastModified, IDictionary<string, string> Metadata) : IDownloadResponse;
public record DownloadFromStreamResponse(Stream Stream, string FileName, string? ContentType, DateTimeOffset? LastModified, IDictionary<string, string> Metadata) : IDownloadResponse;

public record ObjectMetadataResult(string Id, string BucketName, string FileName, DateTimeOffset CreatedTime, DateTimeOffset LastModified, IDictionary<string, string> Metadata);

public record BucketSummary(string Id, Guid Guid, DateTimeOffset CreatedTime);
public record ObjectSummary(string Id, string BucketName, string FileName, DateTimeOffset CreatedTime, DateTimeOffset LastModified, IDictionary<string, string> Metadata);

public record UploadObjectInfo(string Bucket, string Id, string FileName, string ContentType, long Size, IDictionary<string, string>? Metadata = null);