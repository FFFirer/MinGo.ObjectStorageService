using System;

using Microsoft.Extensions.Options;

namespace Mingo.ObjectStorageService;

public class AppOptions
{
    public int AdminPort { get; set; } = 8000;

    public const string ConfiguPath = "App";
}
