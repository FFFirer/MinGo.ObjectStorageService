using System;

using Microsoft.EntityFrameworkCore;

using Mingo.ObjectStorageService.EntityFrameworkCore;

namespace Mingo.ObjectStorageService;

public static class Extensions
{
    public static void Migrate(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Database.Migrate();
    }
}
