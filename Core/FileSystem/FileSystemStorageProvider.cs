using System;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Mingo.ObjectStorageService.Core.Services;

namespace Mingo.ObjectStorageService.Core.FileSystem;

public class FileSystemStorageProvider : IStorageProvider
{
    private readonly FileSystemStorageProviderOptions _options;
    private readonly ILogger _logger;

    public FileSystemStorageProvider(IOptions<FileSystemStorageProviderOptions> options, ILogger<FileSystemStorageProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task DeleteAsync(string bucket, string id, CancellationToken cancellationToken)
    {
        var physicalPath = ResolveObjectPhysicalPath(bucket, id);
        if (File.Exists(physicalPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            File.Delete(physicalPath);

            LogDeletedObject(bucket, id, physicalPath);
        }

        return Task.CompletedTask;
    }

    private void LogDeletedObject(string bucket, string id, string physicalPath)
    {
        _logger.LogDebug("Deleted ({bucket}, {id}): {path}", bucket, id, physicalPath);
    }

    private string? _fullBase;
    protected virtual string FullBasePath
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_fullBase))
            {
                ArgumentNullException.ThrowIfNullOrWhiteSpace(_options.BaseDirectory, $"{FileSystemStorageProviderOptions.Base}:{nameof(FileSystemStorageProviderOptions.BaseDirectory)}");
                _fullBase = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, _options.BaseDirectory));
            }
            return _fullBase;
        }
    }

    private string ResolveObjectPhysicalPath(string bucket, string id)
    {
        var fullPath = Path.GetFullPath(Path.Combine(FullBasePath, bucket, id));
        ThrowIfInvalidPath(fullPath);

        return fullPath;
    }

    private void ThrowIfInvalidPath(string fullPath)
    {
        if (fullPath.StartsWith(FullBasePath, StringComparison.InvariantCultureIgnoreCase) == false)
        {
            throw new InvalidOperationException($"Invalid path");
        }
    }

    public Task EnsureBucketCreatedAsync(string bucketName, CancellationToken cancellationToken)
    {
        var fullPath = ResolveBucketPhysicalPath(bucketName);
        if (Directory.Exists(fullPath))
        {
            return Task.CompletedTask;
        }

        Directory.CreateDirectory(fullPath);
        return Task.CompletedTask;
    }

    private string ResolveBucketPhysicalPath(string bucketName)
    {
        var fullpath = Path.GetFullPath(Path.Combine(FullBasePath, bucketName));
        ThrowIfInvalidPath(fullpath);

        return fullpath;
    }

    public Task EnsureBucketDeletedAsync(string bucketName, CancellationToken cancellationToken)
    {
        var fullPath = ResolveBucketPhysicalPath(bucketName);
        if (Directory.Exists(fullPath))
        {
            var existsFiles = Directory.EnumerateFiles(fullPath).Any();
            if (existsFiles)
            {
                throw new InvalidOperationException("Cannot remove dir with files");
            }

            Directory.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> EnsureBucketIsEmptyAsync(string bucketName, CancellationToken cancellationToken)
    {
        var fullPath = ResolveBucketPhysicalPath(bucketName);
        if (Directory.Exists(fullPath) == false) { return Task.FromResult(true); }

        var existsFiles = Directory.EnumerateFiles(fullPath).Any();
        return Task.FromResult(existsFiles == false);
    }

    public Task<IObjectStorageInfo?> GetObjectAsync(string bucket, string id, CancellationToken cancellationToken)
    {
        IObjectStorageInfo? info = null;

        var fullPath = ResolveObjectPhysicalPath(bucket, id);
        if (File.Exists(fullPath))
        {
            info = new FileSystemStorageInfo(fullPath);
        }

        return Task.FromResult(info);
    }

    public async Task SaveAsync(Stream stream, ObjectInfo objectInfo, CancellationToken cancellationToken)
    {
        var finalPath = ResolveObjectPhysicalPath(objectInfo.Bucket, objectInfo.Id);
        var tempPath = finalPath + ".tmp";

        using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await stream.CopyToAsync(fs, cancellationToken);
        }

        File.Move(tempPath, finalPath, overwrite: true);
    }

    private Stream OpenWriteStream(ObjectInfo objectInfo)
    {
        var path = ResolveObjectPhysicalPath(objectInfo.Bucket, objectInfo.Id);
        var tempPath = path + ".tmp";

        return new AtomicWriteStream(tempPath, path);
    }

    public Stream OpenWriteStream(ObjectInfo objectInfo, CancellationToken cancellationToken)
    {
        return OpenWriteStream(objectInfo);
    }

    private sealed class AtomicWriteStream(string tempPath, string finalPath) : FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None)
    {
        private bool _committed;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_committed && disposing)
            {
                File.Move(tempPath, finalPath, overwrite: true);
                _committed = true;
            }
        }
    }
}
