namespace Mingo.ObjectStorageService.Sdk;

public class OssPageResult<T>
{
    public List<T> Datas { get; set; } = [];
    public int TotalCount { get; set; }
}
