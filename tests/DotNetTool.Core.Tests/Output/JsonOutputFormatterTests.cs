using System.Text.Json;
using DotNetTool.Core.Models;
using DotNetTool.Core.Output;
using FluentAssertions;

namespace DotNetTool.Core.Tests.Output;

public class JsonOutputFormatterTests
{
    private static SolutionInfo BuildSolutionWithTwoProjects()
    {
        var classes1 = new List<ClassInfo>
        {
            new("OrderService", "MyApp.Core.Services.OrderService", false, null)
        };
        var namespaces1 = new List<NamespaceInfo>
        {
            new("MyApp.Core.Services", classes1)
        };

        var project1 = new ProjectInfo(
            Name: "MyApp.Core",
            ProjectType: "Class Library",
            FilePath: "/repos/MyApp/src/MyApp.Core/MyApp.Core.csproj",
            RelativePath: "src/MyApp.Core/MyApp.Core.csproj",
            Namespaces: namespaces1,
            LoadWarnings: []);

        var project2 = new ProjectInfo(
            Name: "MyApp.Cli",
            ProjectType: "Console Application",
            FilePath: "/repos/MyApp/src/MyApp.Cli/MyApp.Cli.csproj",
            RelativePath: "src/MyApp.Cli/MyApp.Cli.csproj",
            Namespaces: [],
            LoadWarnings: []);

        return new SolutionInfo(
            FilePath: "/repos/MyApp/MyApp.sln",
            Name: "MyApp",
            Projects: [project1, project2]);
    }

    [Fact]
    public void Format_WithTwoProjects_ProducesValidJson()
    {
        var formatter = new JsonOutputFormatter();
        var solution = BuildSolutionWithTwoProjects();

        var output = formatter.Format(solution);

        var action = () => JsonDocument.Parse(output);
        action.Should().NotThrow();
    }

    [Fact]
    public void Format_WithTwoProjects_JsonContainsTwoProjects()
    {
        var formatter = new JsonOutputFormatter();
        var solution = BuildSolutionWithTwoProjects();

        var output = formatter.Format(solution);
        using var doc = JsonDocument.Parse(output);

        doc.RootElement.GetProperty("projects").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public void Format_WithTwoProjects_JsonContainsCorrectFullyQualifiedClassName()
    {
        var formatter = new JsonOutputFormatter();
        var solution = BuildSolutionWithTwoProjects();

        var output = formatter.Format(solution);
        using var doc = JsonDocument.Parse(output);

        var firstProject = doc.RootElement.GetProperty("projects")[0];
        var firstNamespace = firstProject.GetProperty("namespaces")[0];
        var firstClass = firstNamespace.GetProperty("classes")[0];

        firstClass.GetProperty("fullyQualifiedName").GetString()
            .Should().Be("MyApp.Core.Services.OrderService");
    }

    [Fact]
    public void Format_OutputIsPrettyPrinted()
    {
        var formatter = new JsonOutputFormatter();
        var solution = BuildSolutionWithTwoProjects();

        var output = formatter.Format(solution);

        output.Should().Contain(Environment.NewLine);
    }
}
