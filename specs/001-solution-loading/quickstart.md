# Quickstart: Solution Loading and Structure Discovery

**Feature**: `001-solution-loading`  
**Date**: 2026-03-17

---

## Prerequisites

- .NET 10 SDK (`dotnet --version` → `10.x.y`)
- The dev container already satisfies this requirement

---

## 1. Clone and Build

```bash
git clone <repo-url>
cd dotnet-tool-experiment
dotnet build
```

---

## 2. Run the Tests

```bash
dotnet test
```

All tests must pass before implementing any new feature (Principle II: Test-First Development).

---

## 3. Install the Tool Locally

```bash
dotnet pack src/DotNetTool.Cli
dotnet tool install --global --add-source src/DotNetTool.Cli/nupkg DotNetTool.Cli
```

Or run directly without installing:

```bash
dotnet run --project src/DotNetTool.Cli -- <path-to-solution>
```

---

## 4. Use the Tool

### List projects in a solution (text, default)

```bash
dotnet-tool /path/to/MyApp.sln
```

Expected output:
```
Solution: MyApp  (/path/to/MyApp.sln)

  MyApp.Core  [Class Library]
    src/MyApp.Core/MyApp.Core.csproj

    Namespaces:
      MyApp.Core.Services
        MyApp.Core.Services.OrderService
      MyApp.Core.Models
        MyApp.Core.Models.Order

  MyApp.Web  [Web Application]
    src/MyApp.Web/MyApp.Web.csproj

    Namespaces:
      MyApp.Web.Controllers
        MyApp.Web.Controllers.HomeController
```

### JSON output

```bash
dotnet-tool /path/to/MyApp.sln --format json
```

### Pipe to jq

```bash
dotnet-tool /path/to/MyApp.sln --format json | jq '[.projects[].name]'
```

---

## 5. Environment Notes

### MSBuild / SDK Discovery

The tool uses `Microsoft.Build.Locator` to find the installed .NET SDK automatically.
If the tool fails to load a solution with a confusing error, verify:

```bash
echo $DOTNET_ROOT   # Should point to the .NET 10 SDK root
dotnet --info       # Shows which SDK is active
```

If `global.json` pins a different SDK version in the solution directory, the tool may
use a different SDK than expected. Override by setting `DOTNET_ROOT` explicitly.

### Cross-Platform Paths

The tool normalises all paths using `Path.GetRelativePath()`. On Windows, backslashes
in `.sln` files are translated to forward slashes in output.

---

## 6. Running Tests with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

Coverage reports are written to `./coverage/`. Open
`./coverage/**/coverage.cobertura.xml` with a compatible viewer or upload to your CI
coverage service.

---

## 7. Project Structure Reference

```
src/
├── DotNetTool.Core/     ← Roslyn analysis library
└── DotNetTool.Cli/      ← CLI entry point (.NET Global Tool)
tests/
├── DotNetTool.Core.Tests/
│   └── Fixtures/        ← Sample .sln files used by unit tests
└── DotNetTool.Integration.Tests/
```

See [plan.md](plan.md) for full architecture details and [data-model.md](data-model.md)
for the entity definitions used throughout the codebase.
