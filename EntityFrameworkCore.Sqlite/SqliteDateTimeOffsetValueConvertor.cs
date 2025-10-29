using System;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Mingo.ObjectStorageService.EntityFrameworkCore.Sqlite;

public class SqliteDateTimeOffsetValueConvertor : ValueConverter<DateTimeOffset, long>
{
    public SqliteDateTimeOffsetValueConvertor() : base(
        v => ConvertToUtcTicks(v),
        v => ConvertToLocalTime(v)
    ) { }

    public static readonly ValueConverter<DateTimeOffset, long> Instance = new SqliteDateTimeOffsetValueConvertor();

    public static long ConvertToUtcTicks(DateTimeOffset value)
    {
        return value.UtcTicks;
    }

    public static DateTimeOffset ConvertToLocalTime(long ticks)
    {
        var time = new DateTimeOffset(ticks, TimeSpan.Zero);
        return time.ToLocalTime();
    }
}
