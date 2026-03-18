using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using DotNetTool.Core.Models;
using NamespaceInfo = DotNetTool.Core.Models.NamespaceInfo;

namespace DotNetTool.Core.Analysis;

public static class ProjectAnalyzer
{
    public static string GetProjectType(Project project)
    {
        // Web application detection: check metadata references for AspNetCore
        var hasAspNetCore = project.MetadataReferences
            .Any(r => r.Display?.Contains("Microsoft.AspNetCore", StringComparison.OrdinalIgnoreCase) == true);

        if (hasAspNetCore)
            return "Web Application";

        // Determine type from compilation options output kind
        var compilationOptions = project.CompilationOptions as CSharpCompilationOptions;
        return compilationOptions?.OutputKind switch
        {
            OutputKind.ConsoleApplication => "Console Application",
            OutputKind.DynamicallyLinkedLibrary => "Class Library",
            OutputKind.WindowsApplication => "Windows Application",
            _ => "Unknown"
        };
    }

    public static async Task<IReadOnlyList<NamespaceInfo>> GetNamespacesAsync(
        Project project,
        CancellationToken ct = default)
    {
        var compilation = await project.GetCompilationAsync(ct);
        if (compilation is null)
            return Array.Empty<NamespaceInfo>();

        var classes = ClassExtractor.Extract(compilation);

        var grouped = classes
            .GroupBy(c =>
            {
                var fqn = c.FullyQualifiedName;
                var lastDot = fqn.LastIndexOf('.');
                if (c.IsNested && c.ContainingTypeName is not null)
                {
                    // For nested classes, namespace is everything before the containing type
                    var dotBeforeContainer = fqn.IndexOf('.' + c.ContainingTypeName + '.' + c.Name,
                        StringComparison.Ordinal);
                    return dotBeforeContainer > 0 ? fqn[..dotBeforeContainer] : "(global)";
                }
                return lastDot > 0 ? fqn[..lastDot] : "(global)";
            })
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => new NamespaceInfo(g.Key, g.ToList()))
            .ToList();

        return grouped;
    }
}
