using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using Mingo.ObjectStorageService.Core.Services;

namespace Mingo.ObjectStorageService.AspNetCore.Endpoints;

public static class ObjectEndpoints
{
    public static IEndpointRouteBuilder MapObjectEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/{bucket}/{**id}", UploadObject);
        endpoints.MapGet("/{bucket}/{**id}", DownloadObject);
        endpoints.MapDelete("/{bucket}/{**id}", DownloadObject);

        return endpoints;
    }

    public static async Task<Ok> UploadObject([FromRoute] string bucket, [FromRoute] string id, [FromBody] IFormFile file, [FromServices] IObjectService objectService, CancellationToken cancellationToken)
    {
        using var stream = file.OpenReadStream();
        await objectService.SaveAsync(stream, new(bucket, id, file.FileName, file.ContentType, file.Length), cancellationToken);
        return TypedResults.Ok();
    }

    public static async Task<Results<IResult, NotFound, InternalServerError>> DownloadObject([FromRoute] string bucket, [FromRoute] string id, [FromServices] IObjectService objectService, CancellationToken cancellationToken)
    {
        var resp = await objectService.GetDownloadAsync(new(bucket, id), cancellationToken);
        
        return resp switch
        {
            null => TypedResults.NotFound(),
            DownloadFromPathResponse dfp => dfp switch
            {
                { IsPhysicalPath: true } => TypedResults.PhysicalFile(dfp.Path, fileDownloadName: dfp.FileName, contentType: dfp.ContentType ?? "application/octet-stream", lastModified: dfp.LastModified),
                _ => TypedResults.VirtualFile(dfp.Path, fileDownloadName: dfp.FileName, contentType: dfp.ContentType ?? "application/octet-stream", lastModified: dfp.LastModified)
            },
            DownloadFromStreamResponse dfs => TypedResults.File(dfs.Stream, fileDownloadName: dfs.FileName, contentType: dfs.ContentType, lastModified: dfs.LastModified),
            _ => TypedResults.InternalServerError()
        };
    }

    public static async Task<Ok> DeleteObject([FromRoute] string bucket, [FromRoute] string id, [FromServices] IObjectService objectService, CancellationToken cancellationToken)
    {
        await objectService.DeleteAsync(new(bucket, id), cancellationToken);
        return TypedResults.Ok();
    }
}
