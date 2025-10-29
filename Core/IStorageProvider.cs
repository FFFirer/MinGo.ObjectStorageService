using System;

using Mingo.ObjectStorageService.Core.Services;

namespace Mingo.ObjectStorageService.Core;

public interface IObjectStorageInfo
{

}

public class FileSystemStorageInfo(string fullPath) : IObjectStorageInfo
{
    public string FullPath { get; init; } = fullPath;
}

public interface IStorageProvider
{
    Task EnsureBucketCreatedAsync(string bucketName, CancellationToken cancellationToken);
    Task<bool> EnsureBucketIsEmptyAsync(string bucketName, CancellationToken cancellationToken);
    Task EnsureBucketDeletedAsync(string bucketName, CancellationToken cancellationToken);

    Task DeleteAsync(string bucket, string id, CancellationToken cancellationToken);
    Task<IObjectStorageInfo?> GetObjectAsync(string bucket, string id, CancellationToken cancellationToken);
    Task SaveAsync(Stream stream, ObjectInfo objectInfo, CancellationToken cancellationToken);
}
