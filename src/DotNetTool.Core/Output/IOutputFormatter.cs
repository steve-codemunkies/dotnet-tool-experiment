using DotNetTool.Core.Models;

namespace DotNetTool.Core.Output;

public interface IOutputFormatter
{
    string Format(SolutionInfo solution);
}
