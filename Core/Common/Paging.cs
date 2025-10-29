using System;

namespace Mingo.ObjectStorageService.Core.Common;

public interface IPageQuery
{
    public int Index { get; }
    public int Size { get; }
}

public class PageQuery : IPageQuery
{
    public PageQuery(int index, int size)
    {
        Index = index;
        Size = size;
    }

    public int Index { get; init; }

    public int Size { get; init; }
}


public class PageResult<T>
{
    public PageResult(List<T> datas, int total)
    {
        Datas = datas;
        TotalCount = total;
    }

    public List<T> Datas { get; init; }
    public int TotalCount { get; init; }

    public static implicit operator PageResult<T>((List<T> datas, int count) result) => new PageResult<T>(result.datas, result.count);
}