using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mingo.ObjectStorageService.EntityFrameworkCore.Sqlite;

public static class Extensions
{
    public static IServiceCollection AddSqliteDbContext<TDbContext>(
        this IServiceCollection serivces,
        IConfiguration configuration,
        string connectionName = "Default",
        Action<DbContextOptionsBuilder>? optionsBuilder = default,
        Action<SqliteDbContextOptionsBuilder>? sqliteBuilder = default)
        where TDbContext : DbContext
    {
        return serivces
        .AddSingleton<IEntityConfiguration<TDbContext>, SqliteEntityConfiguration<TDbContext>>()
        .AddDbContext<TDbContext>(options =>
        {
            optionsBuilder?.Invoke(options);
            options.UseSqlite(configuration.GetConnectionString(connectionName), sqlite =>
            {
                sqlite.MigrationsAssembly("Mingo.ObjectStorageService.EntityFrameworkCore.Sqlite");

                sqliteBuilder?.Invoke(sqlite);
            });
        });
    }

}
