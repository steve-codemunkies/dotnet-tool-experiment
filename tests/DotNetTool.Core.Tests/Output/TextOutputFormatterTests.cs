using DotNetTool.Core.Models;
using DotNetTool.Core.Output;
using FluentAssertions;

namespace DotNetTool.Core.Tests.Output;

public class TextOutputFormatterTests
{
    private static SolutionInfo BuildTwoProjectSolution()
    {
        var project1 = new ProjectInfo(
            Name: "MyConsoleApp",
            ProjectType: "Console Application",
            FilePath: "/repos/MySolution/src/MyConsoleApp/MyConsoleApp.csproj",
            RelativePath: "src/MyConsoleApp/MyConsoleApp.csproj",
            Namespaces: [],
            LoadWarnings: []);

        var project2 = new ProjectInfo(
            Name: "MyLib",
            ProjectType: "Class Library",
            FilePath: "/repos/MySolution/src/MyLib/MyLib.csproj",
            RelativePath: "src/MyLib/MyLib.csproj",
            Namespaces: [],
            LoadWarnings: ["Could not load reference 'SomeMissingPkg'"]);

        return new SolutionInfo(
            FilePath: "/repos/MySolution/MySolution.sln",
            Name: "MySolution",
            Projects: [project1, project2]);
    }

    [Fact]
    public void Format_WithTwoProjects_ContainsProjectNames()
    {
        var formatter = new TextOutputFormatter();
        var solution = BuildTwoProjectSolution();

        var output = formatter.Format(solution);

        output.Should().Contain("MyConsoleApp");
        output.Should().Contain("MyLib");
    }

    [Fact]
    public void Format_WithTwoProjects_ContainsProjectTypesInBrackets()
    {
        var formatter = new TextOutputFormatter();
        var solution = BuildTwoProjectSolution();

        var output = formatter.Format(solution);

        output.Should().Contain("[Console Application]");
        output.Should().Contain("[Class Library]");
    }

    [Fact]
    public void Format_WithTwoProjects_ContainsRelativePaths()
    {
        var formatter = new TextOutputFormatter();
        var solution = BuildTwoProjectSolution();

        var output = formatter.Format(solution);

        output.Should().Contain("src/MyConsoleApp/MyConsoleApp.csproj");
        output.Should().Contain("src/MyLib/MyLib.csproj");
    }

    [Fact]
    public void Format_WithProjectWithNoNamespaces_ShowsNoPubicClassesLabel()
    {
        var formatter = new TextOutputFormatter();
        var solution = BuildTwoProjectSolution();

        var output = formatter.Format(solution);

        output.Should().Contain("(no public classes)");
    }

    [Fact]
    public void Format_WithProjectWithWarnings_WarningsAreInOutput()
    {
        var formatter = new TextOutputFormatter();
        var solution = BuildTwoProjectSolution();

        var output = formatter.Format(solution);

        output.Should().Contain("[warn]");
        output.Should().Contain("Could not load reference 'SomeMissingPkg'");
    }

    // --- Namespace / Class phase (US2) ---

    private static SolutionInfo BuildSolutionWithNamespaces()
    {
        var classes1 = new List<ClassInfo>
        {
            new("OrderService", "MyApp.Core.Services.OrderService", false, null),
            new("UserService",  "MyApp.Core.Services.UserService",  false, null)
        };
        var classes2 = new List<ClassInfo>
        {
            new("Widget", "MyApp.UI.Widget", false, null)
        };

        var namespaces = new List<NamespaceInfo>
        {
            new("MyApp.Core.Services", classes1),
            new("MyApp.UI",            classes2)
        };

        var projectWithNs = new ProjectInfo(
            Name: "MyApp.Core",
            ProjectType: "Class Library",
            FilePath: "/repos/MyApp/src/MyApp.Core/MyApp.Core.csproj",
            RelativePath: "src/MyApp.Core/MyApp.Core.csproj",
            Namespaces: namespaces,
            LoadWarnings: []);

        var projectEmpty = new ProjectInfo(
            Name: "EmptyLib",
            ProjectType: "Class Library",
            FilePath: "/repos/MyApp/src/EmptyLib/EmptyLib.csproj",
            RelativePath: "src/EmptyLib/EmptyLib.csproj",
            Namespaces: [],
            LoadWarnings: []);

        return new SolutionInfo(
            FilePath: "/repos/MyApp/MyApp.sln",
            Name: "MyApp",
            Projects: [projectWithNs, projectEmpty]);
    }

    [Fact]
    public void Format_WithNamespaces_ContainsNamespaceNames()
    {
        var formatter = new TextOutputFormatter();
        var solution = BuildSolutionWithNamespaces();

        var output = formatter.Format(solution);

        output.Should().Contain("MyApp.Core.Services");
        output.Should().Contain("MyApp.UI");
    }

    [Fact]
    public void Format_WithNamespaces_ContainsFullyQualifiedClassNames()
    {
        var formatter = new TextOutputFormatter();
        var solution = BuildSolutionWithNamespaces();

        var output = formatter.Format(solution);

        output.Should().Contain("MyApp.Core.Services.OrderService");
        output.Should().Contain("MyApp.Core.Services.UserService");
        output.Should().Contain("MyApp.UI.Widget");
    }

    [Fact]
    public void Format_WithNamespaces_EmptyProjectShowsNoPubicClassesLabel()
    {
        var formatter = new TextOutputFormatter();
        var solution = BuildSolutionWithNamespaces();

        var output = formatter.Format(solution);

        output.Should().Contain("(no public classes)");
    }
}
