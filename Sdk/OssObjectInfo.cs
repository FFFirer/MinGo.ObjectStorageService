namespace Mingo.ObjectStorageService.Sdk;

public class OssObjectInfo
{
    public string Id { get; set; } = "";
    public string BucketName { get; set; } = "";
    public string FileName { get; set; } = "";
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
