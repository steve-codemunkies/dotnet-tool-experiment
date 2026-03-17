using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using CoreSolutionInfo = DotNetTool.Core.Models.SolutionInfo;
using CoreProjectInfo = DotNetTool.Core.Models.ProjectInfo;
using NamespaceInfo = DotNetTool.Core.Models.NamespaceInfo;

namespace DotNetTool.Core.Analysis;

public static class SolutionLoader
{
    public static async Task<CoreSolutionInfo> LoadAsync(
        string solutionPath,
        IProgress<string>? diagnosticProgress = null,
        CancellationToken ct = default)
    {
        if (!File.Exists(solutionPath))
            throw new FileNotFoundException($"Solution file not found: {solutionPath}", solutionPath);

        var properties = new Dictionary<string, string>
        {
            ["SkipUnrecognizedProjects"] = "true"
        };

        using var workspace = MSBuildWorkspace.Create(properties);

        if (diagnosticProgress is not null)
            workspace.RegisterWorkspaceFailedHandler(e =>
                diagnosticProgress.Report(e.Diagnostic.Message));

        var solution = await workspace.OpenSolutionAsync(solutionPath, cancellationToken: ct);

        var solutionDir = Path.GetDirectoryName(solutionPath)!;
        var solutionName = Path.GetFileNameWithoutExtension(solutionPath);

        var projects = new List<CoreProjectInfo>();
        foreach (var project in solution.Projects)
        {
            var projectType = ProjectAnalyzer.GetProjectType(project);
            var filePath = project.FilePath ?? string.Empty;
            var relativePath = Path.GetRelativePath(solutionDir, filePath)
                                   .Replace('\\', '/');

            var warnings = workspace.Diagnostics
                .Where(d => d.Message.Contains(project.Name, StringComparison.Ordinal))
                .Select(d => d.Message)
                .ToList();

            projects.Add(new CoreProjectInfo(
                Name: project.Name,
                ProjectType: projectType,
                FilePath: filePath,
                RelativePath: relativePath,
                Namespaces: await ProjectAnalyzer.GetNamespacesAsync(project, ct),
                LoadWarnings: warnings));
        }

        return new CoreSolutionInfo(
            FilePath: solutionPath,
            Name: solutionName,
            Projects: projects);
    }
}

