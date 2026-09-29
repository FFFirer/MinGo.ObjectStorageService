namespace Mingo.ObjectStorageService.Sdk;

public class OssClientOptions
{
    public string BaseAddress { get; set; } = "";

    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(10);
}
