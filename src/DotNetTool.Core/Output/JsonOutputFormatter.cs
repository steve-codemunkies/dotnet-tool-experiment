using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetTool.Core.Models;

namespace DotNetTool.Core.Output;

public class JsonOutputFormatter : IOutputFormatter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string Format(SolutionInfo solution)
    {
        return JsonSerializer.Serialize(solution, Options);
    }
}
