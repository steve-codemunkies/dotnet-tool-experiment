# Research: Solution Loading and Structure Discovery

**Feature**: `001-solution-loading`  
**Date**: 2026-03-17  
**Status**: Complete — all NEEDS CLARIFICATION items resolved

---

## 1. Target .NET Version

**Decision**: .NET 10.0 (SDK 10.0.200, confirmed present in dev container)  
**Rationale**: Current SDK in the dev environment; LTS release cadence means .NET 10 is the stable choice for tooling built in early 2026.  
**Alternatives considered**: .NET 8 LTS (still supported until Nov 2026) — rejected because the installed SDK is .NET 10 and targeting an older TFM adds unnecessary friction.

---

## 2. Roslyn Package Selection

**Decision**: `Microsoft.CodeAnalysis.Workspaces.MSBuild` **5.3.0**  
**Rationale**: Latest stable version as of March 2026 with .NET 10 workspace support. Ships Roslyn compiler APIs and the `MSBuildWorkspace` type needed to open `.sln` files.  
**Companion package**: `Microsoft.CodeAnalysis.CSharp.Workspaces` (transitively included by MSBuildWorkspace).

---

## 3. MSBuild Locator

**Decision**: `Microsoft.Build.Locator` **1.11.2**  
**Rationale**: Latest stable; discovers the installed .NET SDK's MSBuild and registers it before any workspace calls.

**Critical constraint**: `MSBuildLocator.RegisterDefaults()` must be called in a **dedicated `Bootstrap()` method** at application entry — before any method that references `Microsoft.CodeAnalysis.MSBuild` or `Microsoft.Build` types. Mixing registration and workspace calls in the same method causes CLR assembly loading failures.

```csharp
// Program.cs — correct ordering
Bootstrap();   // calls MSBuildLocator.RegisterDefaults()
await RunAsync(args);  // System.CommandLine dispatch (references MSBuildWorkspace here)

static void Bootstrap() => MSBuildLocator.RegisterDefaults();
```

**SDK discovery order on Linux**:
`DOTNET_ROOT` → current process path → `DOTNET_HOST_PATH` → `PATH`

---

## 4. System.CommandLine

**Decision**: `System.CommandLine` **2.0.5**  
**Rationale**: First-party Microsoft CLI library; stable release; native async + `CancellationToken` support; `Argument<T>` + `Option<T>` provide strong typing.

**CLI schema for this feature**:
```csharp
var solutionArg = new Argument<FileInfo>("solution")
{
    Description = "Path to the .NET solution file (.sln)"
};
var formatOption = new Option<string>("--format", () => "text")
{
    Description = "Output format: text (default) or json"
};
var rootCommand = new RootCommand("Inspect .NET solution structure");
rootCommand.AddArgument(solutionArg);
rootCommand.AddOption(formatOption);
```

---

## 5. Roslyn Workspace API Patterns

### 5a. Opening a Solution

```csharp
using var workspace = MSBuildWorkspace.Create();
workspace.SkipUnrecognizedProjects = true;  // resilience on Linux

var solution = await workspace.OpenSolutionAsync(
    solutionPath,
    progress: new Progress<ProjectLoadProgress>(p =>
    {
        if (p.LoadOperation == ProjectLoadOperation.Evaluation)
            Console.Error.WriteLine($"[load] {p.ProjectDisplayName}");
    }),
    cancellationToken: ct
);
```

### 5b. Project Type Detection

`CSharpCompilationOptions.OutputKind` is the source of truth for project output type.
Web projects cannot be distinguished from libraries via `OutputKind` alone — they are both
`DynamicallyLinkedLibrary`. Web projects are identified by the presence of an
`Microsoft.AspNetCore.*` metadata reference:

```csharp
string GetProjectType(Project project)
{
    bool isWeb = project.MetadataReferences
        .Any(r => r.Display?.Contains("Microsoft.AspNetCore") == true);
    if (isWeb) return "Web Application";

    if (project.CompilationOptions is CSharpCompilationOptions opts)
    {
        return opts.OutputKind switch
        {
            OutputKind.ConsoleApplication        => "Console Application",
            OutputKind.DynamicallyLinkedLibrary  => "Class Library",
            OutputKind.WindowsApplication        => "Windows Application",
            OutputKind.WindowsRuntimeApplication => "WinRT Application",
            _                                    => "Unknown"
        };
    }
    return "Unknown";
}
```

### 5c. Public Class Extraction

Recursive traversal of `compilation.GlobalNamespace` — no regex, no file parsing:

```csharp
IEnumerable<INamedTypeSymbol> ExtractPublicClasses(INamespaceSymbol ns)
{
    foreach (var member in ns.GetMembers())
    {
        if (member is INamespaceSymbol childNs)
        {
            foreach (var t in ExtractPublicClasses(childNs))
                yield return t;
        }
        else if (member is INamedTypeSymbol type &&
                 type.TypeKind == TypeKind.Class &&
                 type.DeclaredAccessibility == Accessibility.Public)
        {
            yield return type;
            // Recurse into nested public classes
            foreach (var nested in type.GetTypeMembers()
                .Where(n => n.TypeKind == TypeKind.Class &&
                            n.DeclaredAccessibility == Accessibility.Public))
                yield return nested;
        }
    }
}
```

---

## 6. Known Linux Gotchas

| Issue | Impact | Mitigation |
|-------|--------|------------|
| `global.json` SDK pinning ignored by Roslyn | Can load wrong SDK | Set explicit MSBuild properties on workspace; document SDK requirements |
| VB.NET projects on Linux | Load failures | Out of scope (spec: C# only); `SkipUnrecognizedProjects = true` handles gracefully |
| Out-of-process Build Host child process cleanup | Resource leak on crash | Use `using` on `MSBuildWorkspace`; wrap in try/finally |
| Misleading diagnostics for missing SDKs | Confusing error messages | Surface `workspace.Diagnostics` to stderr verbosely |
| `RegisterDefaults()` + MSBuild types in same method | CLR load failure | Enforce `Bootstrap()` pattern (see §3) |

---

## 7. JSON Output

**Decision**: `System.Text.Json` (built-in, .NET 10)  
**Rationale**: Zero additional NuGet dependency; `JsonSerializerOptions.WriteIndented = true` produces human-readable JSON; fully serializable from plain C# model records.  
**Alternatives considered**: `Newtonsoft.Json` — rejected; no advantage over built-in for a simple serialisation task.

---

## 8. Output Format: Text

Indented tree with Unicode box-drawing characters for hierarchy:

```
Solution: MyApp.sln
├── MyApp.Core  [Class Library]  src/MyApp.Core/MyApp.Core.csproj
│   ├── MyApp.Core.Services
│   │   └── OrderService
│   └── MyApp.Core.Models
│       ├── Order
│       └── Customer
└── MyApp.Web  [Web Application]  src/MyApp.Web/MyApp.Web.csproj
    └── MyApp.Web.Controllers
        └── HomeController
```

---

*All NEEDS CLARIFICATION items from Technical Context are now resolved.*
