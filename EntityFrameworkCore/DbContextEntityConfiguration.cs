using System;

using Microsoft.EntityFrameworkCore;

namespace Mingo.ObjectStorageService.EntityFrameworkCore;

public interface IEntityConfiguration<T> : IEntityConfiguration
{

}

public interface IEntityConfiguration
{
    void Configure(ModelBuilder modelBuilder);
}

public abstract class DbContextEntityConfiguration<TDbContext>(string? schema, string? tablePrefix) : IEntityConfiguration<TDbContext>
{
    public DbContextEntityConfiguration() : this(default, default) { }

    public string? Schema { get; init; } = schema;
    public string? TablePrefix { get; init; } = tablePrefix;

    public abstract void Configure(ModelBuilder modelBuilder);
}