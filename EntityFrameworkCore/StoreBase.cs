using System;

using Microsoft.EntityFrameworkCore;

using Mingo.ObjectStorageService.Core.Stores;

namespace Mingo.ObjectStorageService.EntityFrameworkCore;

public abstract class StoreBase : IStoreBase, IDisposable
{
    private bool _disposed;
    protected virtual AppDbContext Db { get; }

    public StoreBase(AppDbContext db)
    {
        Db = db;
    }

    protected virtual void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(this.GetType().FullName);
        }
    }

    public void Dispose() => _disposed = true;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await Db.SaveChangesAsync(cancellationToken);
    }
}
