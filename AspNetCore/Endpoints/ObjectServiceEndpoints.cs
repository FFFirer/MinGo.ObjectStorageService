
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

using Mingo.ObjectStorageService.Core;
using Mingo.ObjectStorageService.Core.Entities;
using Mingo.ObjectStorageService.Core.Services;

namespace Mingo.ObjectStorageService.AspNetCore.Endpoints;

public static class ObjectServiceEndpointExtensions
{
    public static IEndpointRouteBuilder MapObjectServiceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("{bucket}", CreateBucket);
        endpoints.MapDelete("{bucket}", DeleteBucket);

        endpoints.MapPut("{bucket}/{*id}", UploadObject)
            .Accepts<Stream>("application/octet-stream")
            .DisableRequestSizeLimit()  // disable Kestrel MaxRequestSizeLimit
            .DisableAntiforgery();

        endpoints.MapPost("{bucket}/{*id}", UploadObjectForm)
            .Accepts<IFormFile>("multipart/form-data")
            .DisableRequestSizeLimit()
            .RequestFormLimits(options =>
            {
                // options.BufferBodyLengthLimit = 500.MiB();
                options.MultipartBodyLengthLimit = 500.MiB();
            })
            .DisableAntiforgery();

        endpoints.MapGet("{bucket}/{*id}", DownloadObject);
        endpoints.MapDelete("{bucket}/{*id}", DeleteObject);

        return endpoints;
    }

    public static async Task<Ok> DeleteBucketOrObject([FromRoute] string bucket, [FromRoute] string id, [FromServices] IObjectStorageService service, CancellationToken cancellationToken)
    {
        return id switch
        {
            null => await DeleteBucket(bucket, service, cancellationToken),
            _ => await DeleteObject(bucket, id, service, cancellationToken)
        };
    }

    public static async Task<Ok> UploadObjectForm([FromRoute] string bucket, [FromRoute] string id, IFormFile file, HttpRequest request, [FromServices] IObjectStorageService service, CancellationToken cancellationToken)
    {
        var metadata = request.Headers.ResolveUploadObjectMetadata();

        var uploadInfo = new UploadObjectInfo(bucket, id, file.FileName, file.ContentType, file.Length, metadata);
        using var writeStream = await service.OpenObjectSaveStreamAsync(uploadInfo, cancellationToken);
        await file.CopyToAsync(writeStream, cancellationToken);

        return TypedResults.Ok();
    }

    public static async Task<Ok> UploadObject([FromRoute] string bucket, [FromRoute] string id, Stream file, HttpRequest request, [FromServices] IObjectStorageService service, CancellationToken cancellationToken)
    {
        // using var stream = file.OpenReadStream();
        var metadata = request.Headers.ResolveUploadObjectMetadata();
        var filename = metadata.GetValueOrDefault(ObjectMetadataKeys.FileName, string.Empty);
        var contentType = metadata.GetValueOrDefault(ObjectMetadataKeys.ContentType, string.Empty);
        var size = request.ContentLength ?? 0;

        UploadObjectInfo uploadInfo = new(bucket, id, filename, contentType, size, metadata);
        await service.SaveObjectAsync(file, uploadInfo, cancellationToken);
        return TypedResults.Ok();
    }

    public static async Task<Results<IResult, NotFound, InternalServerError>> DownloadObject([FromRoute] string bucket, [FromRoute] string id, [FromServices] IObjectStorageService service, CancellationToken cancellationToken)
    {
        var resp = await service.GetObjectDownloadAsync(new(bucket, id), cancellationToken);

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

    public static async Task<Ok> DeleteObject([FromRoute] string bucket, [FromRoute] string id, [FromServices] IObjectStorageService service, CancellationToken cancellationToken)
    {
        await service.DeleteObjectAsync(new(bucket, id), cancellationToken);
        return TypedResults.Ok();
    }


    public static async Task<Ok> CreateBucket([FromRoute] string bucket, [FromServices] IObjectStorageService service, CancellationToken cancellationToken)
    {
        await service.CreateBucketAsync(bucket, cancellationToken);
        return TypedResults.Ok();
    }

    public static async Task<Ok> DeleteBucket([FromRoute] string bucket, [FromServices] IObjectStorageService service, CancellationToken cancellationToken)
    {
        await service.DeleteBucketAsync(bucket, cancellationToken);
        return TypedResults.Ok();
    }
}
