# Implementation Plan: Solution Loading and Structure Discovery

**Branch**: `001-solution-loading` | **Date**: 2026-03-17 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-solution-loading/spec.md`

## Summary

Build a .NET 10 global CLI tool that opens a .NET solution file using the Roslyn
MSBuildWorkspace API, lists all contained projects with their types, extracts all
public classes organised by namespace, and renders the result as either human-readable
text or JSON. All .NET analysis is implemented through the Roslyn Compiler Platform
(Principle IV); text/regex parsing of `.sln`/`.csproj`/`.cs` files is prohibited.

## Technical Context

**Language/Version**: C# 13 / .NET 10.0  
**Primary Dependencies**:  
  - `Microsoft.CodeAnalysis.Workspaces.MSBuild` 5.3.0 — Roslyn solution/project workspace  
  - `Microsoft.Build.Locator` 1.11.2 — locates the installed .NET SDK for MSBuild  
  - `System.CommandLine` 2.0.5 — CLI argument/option parsing  
  - `xUnit` + `FluentAssertions` + `coverlet.collector` — test framework  
**Storage**: N/A — read-only static analysis, no persistence  
**Testing**: `dotnet test` (xUnit), sample fixture solutions in `tests/`  
**Target Platform**: Cross-platform (.NET 10 on Linux, macOS, Windows)  
**Project Type**: .NET Global Tool (CLI)  
**Performance Goals**: Whole-solution load + class extraction < 5 seconds for ≤ 100 projects  
**Constraints**: No GUI, no interactive prompts; fully automation-friendly (pipe-safe output)  
**Scale/Scope**: Solutions up to 100+ projects; full public-class enumeration per project

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Status | Notes |
|-----------|------|--------|-------|
| I. CLI-Driven Design | All functionality accessible via CLI; text + JSON output | ✅ PASS | FR-010 mandates CLI; FR-008/FR-009 mandate both output formats |
| II. Test-First Development | Tests written and failing BEFORE implementation in every phase | ✅ PASS | Each implementation phase begins with failing test tasks |
| III. Semantic Versioning | Tool packaged with proper version metadata | ✅ PASS | Version applied at package level; changelog maintained per release |
| IV. Roslyn-Powered .NET Analysis | All .sln/.csproj/.cs analysis via Roslyn APIs exclusively | ✅ PASS | `MSBuildWorkspace` + Roslyn symbol APIs throughout; zero text/regex parsing |

**Post-Design Re-check**: ✅ All gates pass. No violations requiring justification.

## Project Structure

### Documentation (this feature)

```text
specs/001-solution-loading/
├── plan.md              # This file
├── research.md          # Phase 0: dependency decisions and Roslyn API patterns
├── data-model.md        # Phase 1: domain entities and relationships
├── quickstart.md        # Phase 1: developer getting-started guide
├── contracts/
│   └── cli-contract.md  # Phase 1: CLI command schema (argument + options)
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created here)
```

### Source Code (repository root)

```text
src/
├── DotNetTool.Core/                     # Roslyn analysis library (no CLI dependency)
│   ├── DotNetTool.Core.csproj
│   ├── Analysis/
│   │   ├── SolutionLoader.cs            # MSBuildWorkspace.OpenSolutionAsync wrapper
│   │   ├── ProjectAnalyzer.cs           # OutputKind → project type; web detection
│   │   └── ClassExtractor.cs           # GlobalNamespace recursive public-class traversal
│   ├── Models/
│   │   ├── SolutionInfo.cs
│   │   ├── ProjectInfo.cs
│   │   ├── NamespaceInfo.cs
│   │   └── ClassInfo.cs
│   └── Output/
│       ├── IOutputFormatter.cs
│       ├── TextOutputFormatter.cs
│       └── JsonOutputFormatter.cs
└── DotNetTool.Cli/                      # .NET Global Tool entry point
    ├── DotNetTool.Cli.csproj
    └── Program.cs                       # System.CommandLine root command wiring

tests/
├── DotNetTool.Core.Tests/               # Unit tests for Core library
│   ├── DotNetTool.Core.Tests.csproj
│   ├── Analysis/
│   │   ├── SolutionLoaderTests.cs
│   │   ├── ProjectAnalyzerTests.cs
│   │   └── ClassExtractorTests.cs
│   ├── Output/
│   │   ├── TextOutputFormatterTests.cs
│   │   └── JsonOutputFormatterTests.cs
│   └── Fixtures/                        # Minimal in-tree .NET solutions for testing
│       ├── SimpleSolution/              # 3 projects: Console, Library, empty
│       └── MultiNamespaceSolution/      # 1 project with classes in 3+ namespaces
└── DotNetTool.Integration.Tests/        # End-to-end CLI invocation tests
    ├── DotNetTool.Integration.Tests.csproj
    └── CliIntegrationTests.cs
```

**Structure Decision**: Two-project `src/` layout (Core library + CLI) cleanly separates
Roslyn analysis from command-line concerns, enabling unit testing of all domain logic
without invoking the CLI layer.

## Architecture

### Layers

```
┌──────────────────────────────────────────┐
│  CLI Layer (DotNetTool.Cli)              │  System.CommandLine root command
│  Program.cs                             │  Argument/option parsing → dispatch
└────────────────┬─────────────────────────┘
                 │
┌────────────────▼─────────────────────────┐
│  Analysis Layer (DotNetTool.Core)        │
│  SolutionLoader  → opens MSBuildWorkspace│
│  ProjectAnalyzer → detects project type  │
│  ClassExtractor  → enumerates symbols    │
└────────────────┬─────────────────────────┘
                 │
┌────────────────▼─────────────────────────┐
│  Model Layer (DotNetTool.Core/Models)    │
│  SolutionInfo, ProjectInfo,              │
│  NamespaceInfo, ClassInfo                │
└────────────────┬─────────────────────────┘
                 │
┌────────────────▼─────────────────────────┐
│  Output Layer (DotNetTool.Core/Output)   │
│  IOutputFormatter                        │
│  TextOutputFormatter / JsonOutputFormatter│
└──────────────────────────────────────────┘
```

### MSBuild Locator Constraint

`MSBuildLocator.RegisterDefaults()` **must** be called in the application entry point
before _any_ method that references MSBuild or Roslyn workspace types, due to CLR
assembly loading order. In `Program.cs` this means the locator call must come first,
in a dedicated `Bootstrap()` method, before `System.CommandLine` dispatch.

## Key Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| .NET analysis API | Roslyn `MSBuildWorkspace` | Mandated by Principle IV; provides correct semantic model |
| Project type detection | `CSharpCompilationOptions.OutputKind` + ASP.NET reference heuristic | Only source-of-truth available without re-invoking MSBuild |
| Web project detection | Check `MetadataReferences` for `Microsoft.AspNetCore` | `OutputKind` alone cannot distinguish Library from Web |
| Class traversal | Recursive `GlobalNamespace.GetMembers()` traversal | Covers all nesting levels and namespaces without regex |
| CLI framework | `System.CommandLine` 2.0.5 | First-party .NET library; native async + cancellation support |
| Output format default | `text` | Human-readable by default; `--format json` for automation |
| Error handling | Per-project `workspace.Diagnostics` inspection; continue on project errors | Matches edge-case requirement: log warning, continue |
| Project structure | Two `src/` projects + two `tests/` projects | Separation of concerns; independent testability of Core |

## Implementation Phases

### Phase 1 — MVP: Load Solution and List Projects (P1)

**Goal**: `load-solution <path>` prints every project name, type, and path.  
**Gate**: Tests written and failing before any implementation begins (Principle II).

**Foundational tasks** (blocking; complete before US1 implementation):
- Scaffold solution, `DotNetTool.Core`, `DotNetTool.Cli`, test projects
- Add NuGet packages: `Microsoft.CodeAnalysis.Workspaces.MSBuild`, `Microsoft.Build.Locator`, `System.CommandLine`, `xUnit`, `FluentAssertions`, `coverlet.collector`
- Configure `DotNetTool.Cli` as a `<PackAsTool>true</PackAsTool>` .NET global tool
- Create `SimpleSolution` and `MultiNamespaceSolution` test fixtures

**US1 test tasks** (write first, all must fail):
- `SolutionLoaderTests`: assert `LoadAsync` returns `SolutionInfo` with correct project count
- `ProjectAnalyzerTests`: assert `OutputKind` → human-readable project type string mapping
- `TextOutputFormatterTests` (projects only): assert formatted output contains project names and types

**US1 implementation tasks**:
- `SolutionInfo`, `ProjectInfo` models
- `SolutionLoader.LoadAsync(string path)` — `MSBuildLocator.RegisterDefaults()` in entry point; `MSBuildWorkspace.Create()` → `OpenSolutionAsync()`
- `ProjectAnalyzer.GetProjectType(Project)` — `OutputKind` + AspNetCore reference check
- `TextOutputFormatter.Format(SolutionInfo)` — project listing with indentation
- `Program.cs` — `Bootstrap()` calls `MSBuildLocator`; root command wires argument → `SolutionLoader` → `TextOutputFormatter`

---

### Phase 2 — List Classes in Projects (P2)

**Goal**: Output includes classes grouped by project and namespace.  
**Gate**: All Phase 1 tests remain green; Phase 2 tests fail before implementation.

**US2 test tasks** (write first, all must fail):
- `ClassExtractorTests`: assert public classes found; private/internal excluded; nested types included when public
- `TextOutputFormatterTests` (with classes): assert namespace grouping and full qualification

**US2 implementation tasks**:
- `NamespaceInfo`, `ClassInfo` models
- `ClassExtractor.Extract(Compilation)` — recursive `GlobalNamespace` traversal via `INamespaceOrTypeSymbol`; filter `TypeKind.Class` + `Accessibility.Public`
- Extend `ProjectAnalyzer` to call `ClassExtractor` after `GetCompilationAsync()`
- Extend `TextOutputFormatter` to render namespace → class hierarchy with indentation
- Empty-project case: project appears in output with `(no public classes)` note

---

### Phase 3 — Export Solution Structure (P3)

**Goal**: `--format json` produces valid, parseable JSON of full hierarchy.  
**Gate**: All Phase 1 + Phase 2 tests remain green; Phase 3 tests fail before implementation.

**US3 test tasks** (write first, all must fail):
- `JsonOutputFormatterTests`: `JsonDocument.Parse()` succeeds; all projects and classes present
- CLI integration test: `--format invalid` exits non-zero with list of supported formats

**US3 implementation tasks**:
- `JsonOutputFormatter.Format(SolutionInfo)` using `System.Text.Json`
- Add `--format <text|json>` option to CLI root command
- Invalid format → `Console.Error` message listing `text`, `json`; exit code 1

## Risk Mitigation

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| MSBuild SDK not found on CI | Medium | Pin `MSBuildLocator.RegisterDefaults()` to first call; document `DOTNET_ROOT` requirement in quickstart |
| `global.json` SDK version mismatch | Low | Set explicit MSBuild properties on workspace creation; document in quickstart |
| Project load failures (diagnostics) | Medium | Use `IProgress<ProjectLoadProgress>` to capture and log diagnostics; set `workspace.SkipUnrecognizedProjects = true` |
| Web project type detection false negatives | Low | Fall back to `Library` with a `(possible web project)` hint; document detection logic |
| Performance > 5 s on large solutions | Low | Benchmark 50-project and 100-project fixtures; `GetCompilationAsync` calls can be made concurrently with `Task.WhenAll` |

## Testing Strategy

| Layer | Framework | Coverage Target | Sample Fixtures |
|-------|-----------|----------------|-----------------|
| Unit — analysis | xUnit + FluentAssertions | ≥ 90% | `SimpleSolution`, `MultiNamespaceSolution` |
| Unit — formatters | xUnit + FluentAssertions | ≥ 95% | In-memory models only |
| Integration | xUnit (process invocation) | key scenarios | `SimpleSolution` fixture |
| E2E (manual) | CLI invocation | acceptance scenarios | Developer's own solutions |

**Integration test approach**: spawn `dotnet run --project src/DotNetTool.Cli -- <fixture-path>` as a subprocess and assert stdout content and exit code.  
**CI gate**: `dotnet test --collect:"XPlat Code Coverage"` with coverage report; PR blocked if overall coverage decreases.

## Dependency Graph (Phase 1 Critical Path)

```
[T001 Scaffold] → [T002 NuGet packages] → [T003 Fixtures]
                                         ↓
                        [T004 Failing tests: SolutionLoader]
                        [T005 Failing tests: ProjectAnalyzer]    ← parallel
                        [T006 Failing tests: TextFormatter]
                                         ↓
               [T007 Models: SolutionInfo, ProjectInfo]
                                         ↓
               [T008 SolutionLoader implementation]
                                         ↓
               [T009 ProjectAnalyzer implementation]
                                         ↓
               [T010 TextOutputFormatter implementation]
                                         ↓
               [T011 CLI Program.cs wiring]
                                         ↓
               [T012 Integration test: project listing]
```
