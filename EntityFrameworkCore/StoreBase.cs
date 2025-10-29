using System;

using Mingo.ObjectStorageService.Core.Stores;

namespace Mingo.ObjectStorageService.EntityFrameworkCore;

public class StoreBase : IStoreBase, IDisposable
{
    protected virtual AppDbContext AppDB { get; init; }
    private bool _disposed;

    public StoreBase(AppDbContext db)
    {
        AppDB = db;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        await AppDB.SaveChangesAsync(cancellationToken);
    }

    protected virtual void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(this.GetType().FullName);
        }
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                AppDB.SaveChanges();
                AppDB.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~StoreBase()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
