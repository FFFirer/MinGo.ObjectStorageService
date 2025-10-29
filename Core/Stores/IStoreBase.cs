using System;

namespace Mingo.ObjectStorageService.Core.Stores;

public interface IStoreBase
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
