using System;

namespace Mingo.ObjectStorageService.Core.Entities;

public class ObjectEntity
{
    public ObjectEntity(string id, string bucketName, string fileName)
    {
        Id = id;
        Guid = Guid.NewGuid();
        BucketName = bucketName;
        FileName = fileName;
        CreatedTime = DateTimeOffset.UtcNow;
    }

    public string Id { get; set; }
    public Guid Guid { get; set; }
    public string BucketName { get; set; }
    public string FileName { get; set; }

    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset LastModified { get; set; }

    public Dictionary<string, string> Metadata { get; set; } = new();
}

public static class ObjectMetadataKeys
{
    public const string ContentType = nameof(ContentType);
}

