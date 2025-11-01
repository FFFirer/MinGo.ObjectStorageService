using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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