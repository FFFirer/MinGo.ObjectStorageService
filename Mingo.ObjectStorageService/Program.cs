using Microsoft.EntityFrameworkCore;

using Mingo.ObjectStorageService;
using Mingo.ObjectStorageService.AspNetCore.Endpoints;
using Mingo.ObjectStorageService.Core;
using Mingo.ObjectStorageService.EntityFrameworkCore;
using Mingo.ObjectStorageService.EntityFrameworkCore.Sqlite;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
.ReadFrom.Configuration(builder.Configuration)
.Enrich.FromLogContext()
.CreateLogger();

builder.Logging
.ClearProviders()
.AddSerilog(Log.Logger);

builder.Services
.AddHttpContextAccessor()
.AddEndpointsApiExplorer()
.AddHealthChecks();
// .AddRazorPages();

builder.Services
.AddOpenApiDocument();

builder.Services
.AddObjectStorageCore()
.AddFileSystemStorageProvider()
.AddSqliteDbContext<AppDbContext>(builder.Configuration)
.AddObjectStorageStores();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.Migrate();
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.MapGroup("oss")
.MapObjectEndpoints()
.MapBucketEndpoints();

app.Run();
