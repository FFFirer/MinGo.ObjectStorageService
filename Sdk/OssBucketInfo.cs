namespace Mingo.ObjectStorageService.Sdk;

public class OssBucketInfo
{
    public string Id { get; set; } = "";
    public Guid Guid { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
}
