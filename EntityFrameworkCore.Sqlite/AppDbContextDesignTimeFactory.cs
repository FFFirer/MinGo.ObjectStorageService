using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mingo.ObjectStorageService.EntityFrameworkCore.Sqlite;

public class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite(sqlite =>
        {
            sqlite.MigrationsAssembly("Mingo.ObjectStorageService.EntityFrameworkCore.Sqlite");
        });
        var entityConfiguration = new SqliteEntityConfiguration<AppDbContext>();

        return new AppDbContext(optionsBuilder.Options, entityConfiguration);
    }
}