namespace Mingo.ObjectStorageService.Core.Stores;

public interface IUnitOfWork : IDisposable
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
