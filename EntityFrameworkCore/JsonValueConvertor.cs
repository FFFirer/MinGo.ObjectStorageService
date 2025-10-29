using System;
using System.Text.Json;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Mingo.ObjectStorageService.EntityFrameworkCore;

public class JsonValueConvertor<T> : ValueConverter<T, string?>
{
    public JsonValueConvertor(T defaultValue) : base(
        v => SerializeToString(v),
        x => DeserializeFromString(x) ?? defaultValue
    )
    {

    }

    public static string? SerializeToString(T value)
    {
        if (value is null) { return default; }

        return JsonSerializer.Serialize(value, default(JsonSerializerOptions));
    }

    public static T? DeserializeFromString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) { return default(T); }
        return JsonSerializer.Deserialize<T?>(value);
    }

    public static ValueConverter<T, string?> Create(T defaultValue = default!)
    {
        return new JsonValueConvertor<T>(defaultValue);
    }
}
