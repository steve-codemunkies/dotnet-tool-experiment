namespace DotNetTool.Core.Models;

public sealed record SolutionInfo(
    string FilePath,
    string Name,
    IReadOnlyList<ProjectInfo> Projects);
