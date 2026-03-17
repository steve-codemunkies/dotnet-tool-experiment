namespace DotNetTool.Core.Models;

public sealed record ClassInfo(
    string Name,
    string FullyQualifiedName,
    bool IsNested,
    string? ContainingTypeName);
