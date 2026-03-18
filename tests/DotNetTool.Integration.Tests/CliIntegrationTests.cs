using System.Diagnostics;
using System.Text.Json;
using FluentAssertions;

namespace DotNetTool.Integration.Tests;

public class CliIntegrationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string CliProjectPath =
        Path.Combine(RepoRoot, "src", "DotNetTool.Cli");

    private static readonly string SimpleSolutionPath =
        Path.Combine(RepoRoot, "tests", "DotNetTool.Core.Tests",
            "Fixtures", "SimpleSolution", "SimpleSolution.sln");

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunCliAsync(
        params string[] args)
    {
        var arguments = string.Join(" ", args.Select(a => $"\"{a}\""));
        var psi = new ProcessStartInfo("dotnet",
            $"run --project \"{CliProjectPath}\" --no-build -- {arguments}")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi)!;
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return (process.ExitCode, stdout, stderr);
    }

    [Fact]
    public async Task Run_WithSimpleSolutionAndFormatJson_ExitsZeroAndOutputsValidJson()
    {
        var (exitCode, stdout, _) = await RunCliAsync(SimpleSolutionPath, "--format", "json");

        exitCode.Should().Be(0);
        var action = () => JsonDocument.Parse(stdout);
        action.Should().NotThrow();
    }

    [Fact]
    public async Task Run_WithInvalidFormat_ExitsOneAndStderrContainsSupportedFormats()
    {
        var (exitCode, _, stderr) = await RunCliAsync(SimpleSolutionPath, "--format", "invalid");

        exitCode.Should().Be(1);
        stderr.Should().Contain("Supported formats");
    }

    [Fact]
    public async Task Run_WithNonExistentSolutionFile_ExitsOneAndStderrContainsNotFound()
    {
        var (exitCode, _, stderr) = await RunCliAsync("/nonexistent/path/missing.sln");

        exitCode.Should().Be(1);
        stderr.Should().Contain("Solution file not found");
    }

    [Fact]
    public async Task Run_WithNonSlnFile_ExitsOneAndStderrContainsExpectedSlnFile()
    {
        var (exitCode, _, stderr) = await RunCliAsync("/tmp/myfile.txt");

        exitCode.Should().Be(1);
        stderr.Should().Contain("Expected a .sln file");
    }
}
