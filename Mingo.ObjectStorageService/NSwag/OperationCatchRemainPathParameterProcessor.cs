using System;
using System.Text.RegularExpressions;

using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Mingo.ObjectStorageService.NSwag;

public class OperationCatchRemainPathParameterProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext processorContext)
    {
        if (processorContext is not AspNetCoreOperationProcessorContext context)
        {
            return false;
        }

        context.OperationDescription.Path = Regex.Replace(context.OperationDescription.Path, "{(.*?)(:(([^/]*)?))?}", match =>
        {
            var pName = match.Groups[1].Value.TrimEnd('?').TrimStart('*').TrimStart('*');
            return "{" + pName + "}";
        });

        return true;
    }
}
