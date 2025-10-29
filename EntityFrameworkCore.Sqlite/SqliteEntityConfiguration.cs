using System;

using Microsoft.EntityFrameworkCore;

using Mingo.ObjectStorageService.Core.Entities;

namespace Mingo.ObjectStorageService.EntityFrameworkCore.Sqlite;

public class SqliteEntityConfiguration<TDbContext> : DbContextEntityConfiguration<TDbContext>
{
    public override void Configure(ModelBuilder modelBuilder)
    {
        var objectEntity = modelBuilder.Entity<ObjectEntity>();
        objectEntity.HasKey(x => new { x.BucketName, x.Id });
        objectEntity.Property(x => x.CreatedTime).HasConversion(SqliteDateTimeOffsetValueConvertor.Instance);
        objectEntity.Property(x => x.LastModified).HasConversion(SqliteDateTimeOffsetValueConvertor.Instance);

        var bucketEntity = modelBuilder.Entity<BucketEntity>();
        bucketEntity.HasKey(x => x.Id);
        bucketEntity.Property(x => x.CreatedTime).HasConversion(SqliteDateTimeOffsetValueConvertor.Instance);
    }
}
