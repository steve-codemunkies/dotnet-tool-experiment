namespace DotNetTool.Core.Models;

public sealed record ProjectInfo(
    string Name,
    string ProjectType,
    string FilePath,
    string RelativePath,
    IReadOnlyList<NamespaceInfo> Namespaces,
    IReadOnlyList<string> LoadWarnings);
