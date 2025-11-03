using System;
using System.Diagnostics.Metrics;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using Mingo.ObjectStorageService.Core.FileSystem;
using Mingo.ObjectStorageService.Core.Services;

namespace Mingo.ObjectStorageService.Core;

public static class Extensions
{
    public static IServiceCollection AddFileSystemStorageProvider(this IServiceCollection services)
    {
        return services
            .AddSingleton<IConfigureOptions<FileSystemStorageProviderOptions>>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                return new ConfigureOptions<FileSystemStorageProviderOptions>(options 
                    => configuration.GetSection(FileSystemStorageProviderOptions.Base)?.Bind(options));
            })
            .AddSingleton<IStorageProvider, FileSystemStorageProvider>();
    }

    public static IServiceCollection AddObjectStorageCore(this IServiceCollection services)
    {
        return services
            .AddScoped<IObjectStorageService, DefaultObjectStorageService>();
    }
}

public static class CollectionExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection is null || collection.Any() == false;
    }

    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));

        return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
    }
}

public static class SizeExtensions
{
    public const long K = 1000;
    public const long M = K * 1000;
    public const long G = M * 1000;
    public const long T = G * 1000;
    public const long P = T * 1000;

    public static long KB(this int size) => size * K;
    public static long MB(this int size) => size * M;
    public static long GB(this int size) => size * G;
    public static long TB(this int size) => size * T;
    public static long PB(this int size) => size * P;
    
    public const long Ki = 1024;
    public const long Mi = Ki * 1024;
    public const long Gi = Mi * 1024;
    public const long Ti = Gi * 1024;
    public const long Pi = Ti * 1024;

    public static long KiB(this int size) => size * Ki;
    public static long MiB(this int size) => size * Mi;
    public static long GiB(this int size) => size * Gi;
    public static long TiB(this int size) => size * Ti;
    public static long PiB(this int size) => size * Pi;
}