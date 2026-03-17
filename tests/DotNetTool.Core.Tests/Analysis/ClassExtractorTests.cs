using DotNetTool.Core.Analysis;
using DotNetTool.Core.Models;
using FluentAssertions;

namespace DotNetTool.Core.Tests.Analysis;

public class ClassExtractorTests
{
    private static readonly string MultiNamespaceSolutionPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "tests", "DotNetTool.Core.Tests", "Fixtures", "MultiNamespaceSolution",
            "MultiNamespaceSolution.sln"));

    [Fact]
    public async Task LoadAsync_WithMultiNamespaceSolution_ReturnsClassesFromAlphaNamespace()
    {
        var solution = await SolutionLoader.LoadAsync(MultiNamespaceSolutionPath);
        var project = solution.Projects.Single(p => p.Name == "MultiLib");

        var allClasses = project.Namespaces.SelectMany(n => n.Classes).ToList();
        allClasses.Should().Contain(c => c.FullyQualifiedName.Contains("Alpha.AlphaRoot"));
    }

    [Fact]
    public async Task LoadAsync_WithMultiNamespaceSolution_ReturnsClassesFromAlphaSubNamespace()
    {
        var solution = await SolutionLoader.LoadAsync(MultiNamespaceSolutionPath);
        var project = solution.Projects.Single(p => p.Name == "MultiLib");

        var allClasses = project.Namespaces.SelectMany(n => n.Classes).ToList();
        allClasses.Should().Contain(c => c.FullyQualifiedName.Contains("Alpha.Sub.AlphaSubClass"));
    }

    [Fact]
    public async Task LoadAsync_WithMultiNamespaceSolution_ReturnsClassesFromBetaNamespace()
    {
        var solution = await SolutionLoader.LoadAsync(MultiNamespaceSolutionPath);
        var project = solution.Projects.Single(p => p.Name == "MultiLib");

        var allClasses = project.Namespaces.SelectMany(n => n.Classes).ToList();
        allClasses.Should().Contain(c => c.FullyQualifiedName.Contains("Beta.BetaClass"));
    }

    [Fact]
    public async Task LoadAsync_WithMultiNamespaceSolution_ExcludesPrivateClass()
    {
        var solution = await SolutionLoader.LoadAsync(MultiNamespaceSolutionPath);
        var project = solution.Projects.Single(p => p.Name == "MultiLib");

        var allClasses = project.Namespaces.SelectMany(n => n.Classes).ToList();
        allClasses.Should().NotContain(c => c.Name == "PrivateBetaClass");
    }

    [Fact]
    public async Task LoadAsync_WithMultiNamespaceSolution_NestedClassHasIsNestedTrue()
    {
        var solution = await SolutionLoader.LoadAsync(MultiNamespaceSolutionPath);
        var project = solution.Projects.Single(p => p.Name == "MultiLib");

        var allClasses = project.Namespaces.SelectMany(n => n.Classes).ToList();
        var nestedClass = allClasses.FirstOrDefault(c => c.Name == "NestedInAlpha");

        nestedClass.Should().NotBeNull();
        nestedClass!.IsNested.Should().BeTrue();
        nestedClass.ContainingTypeName.Should().Be("AlphaContainer");
    }
}
