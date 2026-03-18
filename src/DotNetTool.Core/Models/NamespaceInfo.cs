namespace DotNetTool.Core.Models;

public sealed record NamespaceInfo(
    string FullName,
    IReadOnlyList<ClassInfo> Classes);
