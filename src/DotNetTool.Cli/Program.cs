using System.CommandLine;
using DotNetTool.Core.Analysis;
using DotNetTool.Core.Output;
using Microsoft.Build.Locator;

// MSBuildLocator.RegisterDefaults() MUST be the very first statement before any
// code that touches MSBuild or Roslyn workspace types is loaded by the CLR.
Bootstrap();

var solutionArg = new Argument<FileInfo>(
    name: "solution",
    description: "Path to the .sln file to analyse");

var formatOption = new Option<string>(
    name: "--format",
    description: "Output format: text (default) or json",
    getDefaultValue: () => "text");

var rootCommand = new RootCommand("Analyse a .NET solution and list its projects and public classes")
{
    solutionArg,
    formatOption
};

rootCommand.SetHandler(async (FileInfo solutionFile, string format) =>
{
    if (!string.Equals(solutionFile.Extension, ".sln", StringComparison.OrdinalIgnoreCase))
    {
        Console.Error.WriteLine($"Error: Expected a .sln file, got '{solutionFile.Extension}'");
        Environment.Exit(1);
    }

    if (!string.Equals(format, "text", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
    {
        Console.Error.WriteLine(
            $"Error: Unsupported format '{format}'. Supported formats: text, json");
        Environment.Exit(1);
    }

    try
    {
        var progress = new Progress<string>(msg => Console.Error.WriteLine(msg));
        var solution = await SolutionLoader.LoadAsync(solutionFile.FullName, progress);

        IOutputFormatter formatter = string.Equals(format, "json", StringComparison.OrdinalIgnoreCase)
            ? new JsonOutputFormatter()
            : new TextOutputFormatter();

        Console.Out.Write(formatter.Format(solution));
    }
    catch (FileNotFoundException ex)
    {
        Console.Error.WriteLine($"Error: Solution file not found: {ex.FileName}");
        Environment.Exit(1);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        Environment.Exit(2);
    }
}, solutionArg, formatOption);

return await rootCommand.InvokeAsync(args);

static void Bootstrap()
{
    MSBuildLocator.RegisterDefaults();
}

