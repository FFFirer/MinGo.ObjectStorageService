using System;

namespace Mingo.ObjectStorageService.Core.Stores;

public interface IStoreBase
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
