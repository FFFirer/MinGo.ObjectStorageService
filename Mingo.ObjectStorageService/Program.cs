using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;

using Mingo.ObjectStorageService;
using Mingo.ObjectStorageService.AspNetCore.Endpoints;
using Mingo.ObjectStorageService.Core;
using Mingo.ObjectStorageService.EntityFrameworkCore;
using Mingo.ObjectStorageService.EntityFrameworkCore.Sqlite;
using Mingo.ObjectStorageService.NSwag;

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
.AddObjectStorageStores();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.Migrate();
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.MapGroup("oss").MapObjectServiceEndpoints();

app.Run();
