using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mingo.ObjectStorageService.Sdk.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOssClient(this IServiceCollection services, Action<OssClientOptions> configure)
    {
        services.Configure(configure);
        services.AddHttpClient<OssClient>((sp, http) =>
        {
            var options = sp.GetRequiredService<IOptions<OssClientOptions>>().Value;
            http.BaseAddress = new Uri(options.BaseAddress);
            http.Timeout = options.RequestTimeout;
        });
        return services;
    }
}
