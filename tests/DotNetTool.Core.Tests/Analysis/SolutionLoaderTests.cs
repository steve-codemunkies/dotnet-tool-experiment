using DotNetTool.Core.Analysis;
using FluentAssertions;

namespace DotNetTool.Core.Tests.Analysis;

public class SolutionLoaderTests
{
    private static readonly string SimpleSolutionPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "tests", "DotNetTool.Core.Tests", "Fixtures", "SimpleSolution", "SimpleSolution.sln"));

    [Fact]
    public async Task LoadAsync_WithSimpleSolution_ReturnsSolutionInfoWithCorrectName()
    {
        var result = await SolutionLoader.LoadAsync(SimpleSolutionPath);

        result.Name.Should().Be("SimpleSolution");
    }

    [Fact]
    public async Task LoadAsync_WithSimpleSolution_ReturnsThreeProjects()
    {
        var result = await SolutionLoader.LoadAsync(SimpleSolutionPath);

        result.Projects.Should().HaveCount(3);
    }

    [Fact]
    public async Task LoadAsync_WithNonExistentPath_ThrowsFileNotFoundException()
    {
        var action = () => SolutionLoader.LoadAsync("/nonexistent/path/missing.sln");

        await action.Should().ThrowAsync<FileNotFoundException>();
    }
}
