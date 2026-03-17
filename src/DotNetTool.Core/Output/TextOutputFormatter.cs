using DotNetTool.Core.Models;

namespace DotNetTool.Core.Output;

public class TextOutputFormatter : IOutputFormatter
{
    public string Format(SolutionInfo solution)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"Solution: {solution.Name}  ({solution.FilePath})");
        sb.AppendLine();

        foreach (var project in solution.Projects)
        {
            sb.AppendLine($"  {project.Name}  [{project.ProjectType}]");
            sb.AppendLine($"    {project.RelativePath}");

            foreach (var warning in project.LoadWarnings)
                sb.AppendLine($"    [warn] {warning}");

            if (project.Namespaces.Count == 0)
            {
                sb.AppendLine("    (no public classes)");
            }
            else
            {
                sb.AppendLine("    Namespaces:");
                foreach (var ns in project.Namespaces)
                {
                    sb.AppendLine($"      {ns.FullName}");
                    foreach (var cls in ns.Classes)
                        sb.AppendLine($"        {cls.FullyQualifiedName}");
                }
            }
        }

        return sb.ToString();
    }
}
