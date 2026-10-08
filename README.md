# Mingo.ObjectStorageService

轻量级对象存储服务，提供上传、下载、存储管理能力。基于 .NET 10 构建，以模块化 NuGet 包形式提供，可按需集成或扩展。

## 架构

```
┌─────────────────────────────────────────────────────┐
│                 Mingo.ObjectStorageService           │  ← Web 应用宿主
├─────────────────────────────────────────────────────┤
│  AspNetCore          │  EntityFrameworkCore.Sqlite   │  ← 基础设施层
│  (API 端点)           │  (SQLite 持久化)               │
├─────────────────────────────────────────────────────┤
│  EntityFrameworkCore │  Sdk                          │  ← 能力层
│  (EF Core Store)     │  (HTTP 客户端)                 │
├─────────────────────────────────────────────────────┤
│                       Core                           │  ← 核心抽象层
│  IObjectStorageService / IStorageProvider / Stores   │
└─────────────────────────────────────────────────────┘
```

## NuGet 包

| 包名 | 说明 |
|------|------|
| `Mingo.ObjectStorageService.Core` | 核心抽象与默认实现（`IObjectStorageService`、`IStorageProvider`、FileSystem 存储） |
| `Mingo.ObjectStorageService.AspNetCore` | Minimal API 端点映射（上传/下载/HEAD/列表/删除） |
| `Mingo.ObjectStorageService.EntityFrameworkCore` | EF Core Store 实现（`IBucketStore`、`IObjectStore`） |
| `Mingo.ObjectStorageService.EntityFrameworkCore.Sqlite` | SQLite 提供程序，含 DbContext 配置与实体映射 |
| `Mingo.ObjectStorageService.Sdk` | HTTP 客户端 SDK（`OssClient`），支持 DI 注册 |

## 快速开始

### 1. 安装 NuGet 包

```bash
dotnet add package Mingo.ObjectStorageService.Core
dotnet add package Mingo.ObjectStorageService.AspNetCore
dotnet add package Mingo.ObjectStorageService.EntityFrameworkCore
dotnet add package Mingo.ObjectStorageService.EntityFrameworkCore.Sqlite
```

### 2. 配置服务

```csharp
// Program.cs
builder.Services.AddObjectStorageCore();                          // 核心服务
builder.Services.AddFileSystemStorageProvider();                   // 文件系统存储
builder.Services.AddSqliteDbContext<AppDbContext>(builder.Configuration); // SQLite 持久化
builder.Services.AddObjectStorageStores();                         // Store 实现

// 端点映射
app.MapGroup("oss").MapObjectServiceEndpoints();
```

### 3. 配置项

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=oss.db"
  },
  "StorageProvider": {
    "FileSystem": {
      "BaseDirectory": "App_Data/oss"
    }
  }
}
```

| 配置项 | 默认值 | 说明 |
|--------|--------|------|
| `ConnectionStrings:Default` | — | SQLite 数据库连接字符串 |
| `StorageProvider:FileSystem:BaseDirectory` | `App_Data/oss` | 文件存储根目录（相对于应用目录） |

## API 端点

所有端点挂载在 `/oss` 前缀下：

| 方法 | 路由 | 说明 |
|------|------|------|
| `GET` | `/oss` | 分页查询 Bucket 列表 |
| `GET` | `/oss/{bucket}` | 分页查询对象列表（支持 `prefix` 过滤） |
| `POST` | `/oss/{bucket}` | 创建 Bucket |
| `DELETE` | `/oss/{bucket}` | 删除 Bucket（需为空） |
| `PUT` | `/oss/{bucket}/{*id}` | 流式上传（`application/octet-stream`，无大小限制） |
| `POST` | `/oss/{bucket}/{*id}` | 表单上传（`multipart/form-data`，500 MiB 限制） |
| `GET` | `/oss/{bucket}/{*id}` | 下载对象 |
| `HEAD` | `/oss/{bucket}/{*id}` | 获取对象元数据（不返回 Body） |
| `DELETE` | `/oss/{bucket}/{*id}` | 删除对象 |

### 自定义元数据

通过 `MINGO-OSS-*` 请求头传递自定义元数据，例如：

```
MINGO-OSS-FileName: report.pdf
MINGO-OSS-ContentType: application/pdf
MINGO-OSS-Author: admin
```

下载时，服务端通过同名响应头返回元数据。

## SDK 使用

### 安装

```bash
dotnet add package Mingo.ObjectStorageService.Sdk
```

### DI 注册

```csharp
builder.Services.AddOssClient(options =>
{
    options.BaseAddress = "https://your-oss-server.com";
    options.RequestTimeout = TimeSpan.FromMinutes(10);
});
```

### 调用示例

```csharp
public class MyService
{
    private readonly OssClient _client;

    public MyService(OssClient client) => _client = client;

    public async Task UploadExample()
    {
        // 创建 Bucket
        await _client.CreateBucketAsync("my-bucket");

        // 上传文件
        await using var fs = File.OpenRead("report.pdf");
        await _client.UploadAsync("my-bucket", "doc-001", fs, "report.pdf",
            "application/pdf",
            metadata: new Dictionary<string, string> { ["Author"] = "admin" });

        // 检查对象是否存在
        bool exists = await _client.ExistsAsync("my-bucket", "doc-001");

        // 获取元数据
        var info = await _client.HeadObjectAsync("my-bucket", "doc-001");

        // 下载为 Stream
        await using var stream = await _client.GetStreamAsync("my-bucket", "doc-001");

        // 下载到本地文件
        await _client.DownloadToFileAsync("my-bucket", "doc-001", "./downloaded.pdf");

        // 分页查询对象
        var list = await _client.ListObjectsAsync("my-bucket", prefix: "doc");

        // 删除对象
        await _client.DeleteObjectAsync("my-bucket", "doc-001");
    }
}
```

## 扩展存储提供程序

实现 `IStorageProvider` 接口即可对接其他存储后端（如 S3、MinIO、Azure Blob）：

```csharp
public class S3StorageProvider : IStorageProvider
{
    public Task EnsureBucketCreatedAsync(string bucketName, CancellationToken cancellationToken) { ... }
    public Task EnsureBucketDeletedAsync(string bucketName, CancellationToken cancellationToken) { ... }
    public Task<bool> EnsureBucketIsEmptyAsync(string bucketName, CancellationToken cancellationToken) { ... }
    public Task DeleteAsync(string bucket, string id, CancellationToken cancellationToken) { ... }
    public Task<IObjectStorageInfo?> GetObjectAsync(string bucket, string id, CancellationToken cancellationToken) { ... }
    public Task SaveAsync(Stream stream, ObjectInfo objectInfo, CancellationToken cancellationToken) { ... }
    public Stream OpenWriteStream(ObjectInfo objectInfo, CancellationToken cancellationToken) { ... }
}
```

注册替换：

```csharp
builder.Services.AddSingleton<IStorageProvider, S3StorageProvider>();
```

## 原子写入

FileSystem 存储提供程序使用原子写入模式：先写入 `.tmp` 临时文件，完成后通过 `File.Move(overwrite: true)` 原子替换目标文件。确保在写入中断时不会产生损坏的对象文件。

## 本地开发

```bash
# 克隆
git clone https://github.com/FFFirer/MinGo.ObjectStorageService.git
cd MinGo.ObjectStorageService

# 还原 & 构建
dotnet restore
dotnet build

# 运行
cd Mingo.ObjectStorageService
dotnet run
```

## 发布 NuGet 包

推送版本 tag 即可触发 GitHub Actions 自动发布：

```bash
# 更新 Directory.Build.props 中的 <Version>
git add -A && git commit -m "Bump version to 1.1.0"
git tag v1.1.0
git push github master --tags
```

## License

MIT
