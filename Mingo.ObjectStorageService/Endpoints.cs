using System;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using Mingo.ObjectStorageService.AspNetCore.Endpoints;
using Mingo.ObjectStorageService.Core.Services;

namespace Mingo.ObjectStorageService;

public static class Endpoints
{
    public static string BuildObjectDownloadUrl(string bucket, string id) => $"/download/{bucket}/{id}";

    public static IEndpointConventionBuilder MapAdminEndpoints(this IEndpointRouteBuilder endpoint)
    {
        return endpoint.MapGet("/download/{bucket}/{*id}", DownloadObject);
    }

    public static Task<Results<IResult, NotFound, InternalServerError>> DownloadObject([FromRoute] string bucket, [FromRoute] string id, HttpResponse response, [FromServices] IObjectStorageService service, CancellationToken cancellationToken)
        => ObjectServiceEndpointExtensions.DownloadObject(bucket, id, response, service, cancellationToken);
}
