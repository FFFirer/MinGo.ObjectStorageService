using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using Mingo.ObjectStorageService.Core.Services;

namespace Mingo.ObjectStorageService.AspNetCore.Endpoints;

public static class BucketEndpoints
{
    public static IEndpointRouteBuilder MapBucketEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/{bucket}", CreateBucket);
        endpoints.MapDelete("/{bucket}", DeleteBucket);

        return endpoints;
    }

    public static async Task<Ok> CreateBucket([FromRoute] string bucket, [FromServices] IBucketService bucketService,CancellationToken cancellationToken)
    {
        await bucketService.CreateAsync(bucket, cancellationToken);
        return TypedResults.Ok();
    }
    
    public static async Task<Ok> DeleteBucket([FromRoute] string bucket, [FromServices] IBucketService bucketService, CancellationToken cancellationToken)
    {
        await bucketService.DeleteAsync(bucket, cancellationToken);
        return TypedResults.Ok();
    }
}
