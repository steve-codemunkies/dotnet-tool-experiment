using DotNetTool.Core.Analysis;
using FluentAssertions;

namespace DotNetTool.Core.Tests.Analysis;

public class ProjectAnalyzerTests
{
    private static readonly string SimpleSolutionPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "tests", "DotNetTool.Core.Tests", "Fixtures", "SimpleSolution", "SimpleSolution.sln"));

    [Fact]
    public async Task LoadAsync_WithSimpleSolution_ConsoleAppHasCorrectProjectType()
    {
        var solution = await SolutionLoader.LoadAsync(SimpleSolutionPath);

        var consoleProject = solution.Projects.Single(p => p.Name == "ConsoleApp");
        consoleProject.ProjectType.Should().Be("Console Application");
    }

    [Fact]
    public async Task LoadAsync_WithSimpleSolution_CoreLibHasCorrectProjectType()
    {
        var solution = await SolutionLoader.LoadAsync(SimpleSolutionPath);

        var coreLib = solution.Projects.Single(p => p.Name == "CoreLib");
        coreLib.ProjectType.Should().Be("Class Library");
    }

    [Fact]
    public async Task LoadAsync_WithSimpleSolution_EmptyLibHasCorrectProjectType()
    {
        var solution = await SolutionLoader.LoadAsync(SimpleSolutionPath);

        var emptyLib = solution.Projects.Single(p => p.Name == "EmptyLib");
        emptyLib.ProjectType.Should().Be("Class Library");
    }
}
