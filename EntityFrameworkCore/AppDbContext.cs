using System;

using Microsoft.EntityFrameworkCore;

using Mingo.ObjectStorageService.Core.Entities;

namespace Mingo.ObjectStorageService.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    private readonly IEntityConfiguration _entityConfiguration;
    public AppDbContext(DbContextOptions<AppDbContext> options, IEntityConfiguration<AppDbContext> entityConfiguration) : base(options)
    {
        _entityConfiguration = entityConfiguration;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _entityConfiguration.Configure(modelBuilder);
    }

    public DbSet<ObjectEntity> Objects => Set<ObjectEntity>();
    public DbSet<BucketEntity> Buckets => Set<BucketEntity>();
}
