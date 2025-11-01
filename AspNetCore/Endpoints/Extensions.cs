using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace Mingo.ObjectStorageService.AspNetCore.Endpoints;

public static class MiniApiExtensions
{
    private readonly static DisableRequestSizeLimitAttribute _disableRequetSizeLimitAttribute = new DisableRequestSizeLimitAttribute();
    public static IEndpointConventionBuilder DisableRequestSizeLimit(this IEndpointConventionBuilder endpoint)
    {
        endpoint.Add(b => b.Metadata.Add(_disableRequetSizeLimitAttribute));
        return endpoint;
    }
}

public static class HttpExtensions
{
    public static IDictionary<string, string> ResolveUploadObjectMetadata(this IHeaderDictionary headers)
    {
        var metadata = headers.Where(x => x.Key.StartsWith("MINGO-OSS-", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.Key.Substring("MINGO-OSS-".Length), x => x.Value.ToString(), StringComparer.OrdinalIgnoreCase);

        return metadata;
    }
}

public class RequestSizeLimitMiddleware
{
    private readonly RequestDelegate _next;

    public RequestSizeLimitMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        var endpoint = httpContext.GetEndpoint();
        var feature = httpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (feature is not null)
        {
            if (endpoint is not null)
            {
                var requestSizeLimit = endpoint.Metadata.Where(x => x is IRequestSizeLimitMetadata).FirstOrDefault();
                if (requestSizeLimit is not null)
                {
                    feature.MaxRequestBodySize = ((IRequestSizeLimitMetadata)requestSizeLimit).MaxRequestBodySize;
                }
                else
                {
                    feature.MaxRequestBodySize = 30_000_000;
                }
            }
        }

        await _next(httpContext);
    }
}
