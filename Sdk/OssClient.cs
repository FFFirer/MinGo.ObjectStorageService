using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Mingo.ObjectStorageService.Sdk;

public class OssClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public OssClient(HttpClient http)
    {
        _http = http;
    }

    #region Bucket

    public async Task CreateBucketAsync(string bucket, CancellationToken ct = default)
    {
        using var resp = await _http.PostAsync(EncodePath(bucket), content: null, ct);
        await EnsureSuccessAsync(resp, ct);
    }

    public async Task DeleteBucketAsync(string bucket, CancellationToken ct = default)
    {
        using var resp = await _http.DeleteAsync(EncodePath(bucket), ct);
        await EnsureSuccessAsync(resp, ct);
    }

    public async Task<OssPageResult<OssBucketInfo>> ListBucketsAsync(int pageIndex = 1, int pageSize = 20, CancellationToken ct = default)
    {
        using var resp = await _http.GetAsync($"?pageIndex={pageIndex}&pageSize={pageSize}", ct);
        await EnsureSuccessAsync(resp, ct);
        return await DeserializeAsync<OssPageResult<OssBucketInfo>>(resp, ct);
    }

    #endregion

    #region Upload

    public async Task UploadAsync(string bucket, string id, Stream stream, string fileName, string contentType, IDictionary<string, string>? metadata = null, CancellationToken ct = default)
    {
        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using var request = new HttpRequestMessage(HttpMethod.Put, EncodePath(bucket, id));
        request.Content = content;
        SetMetadataHeaders(request, fileName, contentType, metadata);

        using var resp = await _http.SendAsync(request, ct);
        await EnsureSuccessAsync(resp, ct);
    }

    public async Task UploadFileAsync(string bucket, string id, string localFilePath, IDictionary<string, string>? metadata = null, CancellationToken ct = default)
    {
        var fileName = Path.GetFileName(localFilePath);
        var contentType = GuessContentType(fileName);

        using var fs = File.OpenRead(localFilePath);
        await UploadAsync(bucket, id, fs, fileName, contentType, metadata, ct);
    }

    #endregion

    #region Download

    public async Task<Stream> GetStreamAsync(string bucket, string id, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync(EncodePath(bucket, id), HttpCompletionOption.ResponseHeadersRead, ct);
        await EnsureSuccessAsync(resp, ct);
        return await resp.Content.ReadAsStreamAsync(ct);
    }

    public async Task DownloadToFileAsync(string bucket, string id, string localFilePath, CancellationToken ct = default)
    {
        var tempPath = localFilePath + ".tmp";

        using (var stream = await GetStreamAsync(bucket, id, ct))
        using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await stream.CopyToAsync(fs, ct);
        }

        File.Move(tempPath, localFilePath, overwrite: true);
    }

    #endregion

    #region Metadata

    public async Task<OssObjectInfo?> HeadObjectAsync(string bucket, string id, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, EncodePath(bucket, id));
        using var resp = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessAsync(resp, ct);

        var headers = resp.Headers;
        var meta = new Dictionary<string, string>();

        foreach (var h in resp.Headers)
        {
            if (h.Key.StartsWith("MINGO-OSS-", StringComparison.OrdinalIgnoreCase))
            {
                var key = h.Key["MINGO-OSS-".Length..];
                meta[key] = string.Join(",", h.Value);
            }
        }
        foreach (var h in resp.Content.Headers)
        {
            if (h.Key.StartsWith("MINGO-OSS-", StringComparison.OrdinalIgnoreCase))
            {
                var key = h.Key["MINGO-OSS-".Length..];
                meta[key] = string.Join(",", h.Value);
            }
        }

        return new OssObjectInfo
        {
            Id = id,
            BucketName = bucket,
            FileName = meta.GetValueOrDefault("FileName", ""),
            LastModified = resp.Content.Headers.LastModified?.LocalDateTime ?? DateTimeOffset.MinValue,
            Metadata = meta,
        };
    }

    public async Task<bool> ExistsAsync(string bucket, string id, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, EncodePath(bucket, id));
        using var resp = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return resp.IsSuccessStatusCode;
    }

    #endregion

    #region List

    public async Task<OssPageResult<OssObjectInfo>> ListObjectsAsync(string bucket, int pageIndex = 1, int pageSize = 20, string? prefix = null, CancellationToken ct = default)
    {
        var url = EncodePath(bucket) + $"?pageIndex={pageIndex}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(prefix))
        {
            url += $"&prefix={Uri.EscapeDataString(prefix)}";
        }

        using var resp = await _http.GetAsync(url, ct);
        await EnsureSuccessAsync(resp, ct);
        return await DeserializeAsync<OssPageResult<OssObjectInfo>>(resp, ct);
    }

    #endregion

    #region Delete

    public async Task DeleteObjectAsync(string bucket, string id, CancellationToken ct = default)
    {
        using var resp = await _http.DeleteAsync(EncodePath(bucket, id), ct);
        await EnsureSuccessAsync(resp, ct);
    }

    #endregion

    #region Helpers

    private static void SetMetadataHeaders(HttpRequestMessage request, string fileName, string contentType, IDictionary<string, string>? metadata)
    {
        request.Headers.TryAddWithoutValidation("MINGO-OSS-FileName", fileName);
        request.Headers.TryAddWithoutValidation("MINGO-OSS-ContentType", contentType);

        if (metadata is not null)
        {
            foreach (var kv in metadata)
            {
                request.Headers.TryAddWithoutValidation($"MINGO-OSS-{kv.Key}", kv.Value);
            }
        }
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"OSS request failed {(int)resp.StatusCode}: {body}");
        }
    }

    private async Task<T> DeserializeAsync<T>(HttpResponseMessage resp, CancellationToken ct)
    {
        using var stream = await resp.Content.ReadAsStreamAsync(ct);
        return (await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, ct))!;
    }

    private static string EncodePath(string bucket) => Uri.EscapeDataString(bucket);

    private static string EncodePath(string bucket, string id) => $"{Uri.EscapeDataString(bucket)}/{id}";

    private static string GuessContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".pdf" => "application/pdf",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".txt" => "text/plain",
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".zip" => "application/zip",
            ".mp4" => "video/mp4",
            ".mp3" => "audio/mpeg",
            _ => "application/octet-stream",
        };
    }

    #endregion
}
