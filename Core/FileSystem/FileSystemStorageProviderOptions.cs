using System;

namespace Mingo.ObjectStorageService.Core.FileSystem;

public class FileSystemStorageProviderOptions
{
    public string BaseDirectory { get; set; } = "App_Data/files";


    public const string Base = "StorageProvider:FileSystem";
}
