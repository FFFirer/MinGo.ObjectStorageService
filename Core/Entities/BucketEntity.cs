using System;

namespace Mingo.ObjectStorageService.Core.Entities;

public class BucketEntity
{
    public BucketEntity(string id)
    {
        Id = id;
        Guid = Guid.NewGuid();
        CreatedTime = DateTimeOffset.UtcNow;
    }

    public string Id { get; set; }
    public Guid Guid { get; set; }
    public DateTimeOffset CreatedTime { get; set; }

    public Dictionary<string, string> Metadata { get; set; } = new();
}
