using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using Mingo.ObjectStorageService;
using Mingo.ObjectStorageService.AspNetCore.Endpoints;
using Mingo.ObjectStorageService.Core;
using Mingo.ObjectStorageService.EntityFrameworkCore;
using Mingo.ObjectStorageService.EntityFrameworkCore.Sqlite;
using Mingo.ObjectStorageService.NSwag;

using Serilog;

using Vite.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    // 注释此行，会影响IFormFile绑定，未匹配到Endpoint就会抛出异常
    // 调用DisableRequestSizeLimit, 效果如下
    // options.Limits.MaxRequestBodySize = null;
});

Log.Logger = new LoggerConfiguration()
.ReadFrom.Configuration(builder.Configuration)
.Enrich.FromLogContext()
.CreateLogger();

builder.Logging
.ClearProviders()
.AddSerilog(Log.Logger);

builder.Services
.AddHttpContextAccessor()
.AddProblemDetails()
.AddEndpointsApiExplorer()
.AddHealthChecks();

// Razor Pages
builder.Services
.AddRazorPages();

// Vite Services
builder.Services
.AddViteServices(v => { v.Server.AutoRun = true; });

builder.Services
.AddOpenApiDocument(doc =>
{
    doc.Title = "ObjectStorage Service APIs";

    doc.OperationProcessors.Insert(1, new OperationCatchRemainPathParameterProcessor());
});


builder.Services
.AddObjectStorageCore()
.AddFileSystemStorageProvider()
.AddSqliteDbContext<AppDbContext>(builder.Configuration)
.AddObjectStorageStores()
.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.ConfiguPath));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebSockets();
    app.UseViteDevelopmentServer(true);

    app.Migrate();
    app.UseOpenApi();
    app.UseSwaggerUi();

    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRouting();

// app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.MapAdminEndpoints();
app.MapGroup("oss").MapObjectServiceEndpoints();

app.Run();
