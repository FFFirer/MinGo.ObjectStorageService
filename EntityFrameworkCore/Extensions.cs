using System;

using Microsoft.Extensions.DependencyInjection;

using Mingo.ObjectStorageService.Core.Stores;

namespace Mingo.ObjectStorageService.EntityFrameworkCore;

public static class Extensions
{
    public static IServiceCollection AddObjectStorageStores(this IServiceCollection services)
    {
        return services
            .AddScoped<IObjectStore, ObjectStore>()
            .AddScoped<IBucketStore, BucketStore>();
    }
}
