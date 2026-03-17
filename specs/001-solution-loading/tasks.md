---
description: "Task list for Solution Loading and Structure Discovery"
---

# Tasks: Solution Loading and Structure Discovery

**Input**: Design documents from `/specs/001-solution-loading/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/cli-contract.md ✅

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.  
**Constitution**: All tasks comply with Principles I–IV (CLI-first, test-first, SemVer, Roslyn-only).  
**Tests**: Test tasks are included — Principle II (Test-First Development) is NON-NEGOTIABLE.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Parallelisable — different files, no dependency on an incomplete sibling task
- **[Story]**: User story label (US1 / US2 / US3)
- Exact file paths are included in every task description

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Scaffold the .NET solution, all projects, NuGet packages, and test fixtures.  
**⚠️ CRITICAL**: No user story work begins until this phase is complete.

- [ ] T001 Initialise .NET solution file: `dotnet new sln -n DotNetTool` at repository root
- [ ] T002 Create `DotNetTool.Core` class library: `dotnet new classlib -n DotNetTool.Core -o src/DotNetTool.Core --framework net10.0` and add to solution
- [ ] T003 Create `DotNetTool.Cli` console project configured as a .NET Global Tool: `dotnet new console -n DotNetTool.Cli -o src/DotNetTool.Cli --framework net10.0`; set `<PackAsTool>true</PackAsTool>`, `<ToolCommandName>dotnet-tool</ToolCommandName>`, `<PackageId>DotNetTool.Cli</PackageId>` in `src/DotNetTool.Cli/DotNetTool.Cli.csproj`; add project reference to `DotNetTool.Core`; add to solution
- [ ] T004 [P] Create `DotNetTool.Core.Tests` xUnit test project: `dotnet new xunit -n DotNetTool.Core.Tests -o tests/DotNetTool.Core.Tests --framework net10.0`; add project reference to `DotNetTool.Core`; add to solution
- [ ] T005 [P] Create `DotNetTool.Integration.Tests` xUnit test project: `dotnet new xunit -n DotNetTool.Integration.Tests -o tests/DotNetTool.Integration.Tests --framework net10.0`; add to solution
- [ ] T006 Add NuGet packages to `src/DotNetTool.Core/DotNetTool.Core.csproj`: `Microsoft.CodeAnalysis.Workspaces.MSBuild` 5.3.0, `Microsoft.Build.Locator` 1.11.2
- [ ] T007 [P] Add NuGet packages to `src/DotNetTool.Cli/DotNetTool.Cli.csproj`: `System.CommandLine` 2.0.5
- [ ] T008 [P] Add NuGet packages to `tests/DotNetTool.Core.Tests/DotNetTool.Core.Tests.csproj`: `FluentAssertions`, `coverlet.collector`
- [ ] T009 [P] Add NuGet packages to `tests/DotNetTool.Integration.Tests/DotNetTool.Integration.Tests.csproj`: `FluentAssertions`, `coverlet.collector`
- [ ] T010 Create `SimpleSolution` test fixture under `tests/DotNetTool.Core.Tests/Fixtures/SimpleSolution/`: a minimal .NET solution with 3 projects — `ConsoleApp` (Console Application), `CoreLib` (Class Library), `EmptyLib` (Class Library with no source files); each project must be a valid SDK-style `.csproj` that builds with `dotnet build`
- [ ] T011 [P] Create `MultiNamespaceSolution` test fixture under `tests/DotNetTool.Core.Tests/Fixtures/MultiNamespaceSolution/`: a minimal .NET solution with 1 Class Library project containing public classes spread across 3 namespaces (`Alpha`, `Alpha.Sub`, `Beta`) and 1 private class (to verify exclusion)
- [ ] T012 Verify `dotnet build` succeeds for the whole solution and `dotnet test` runs (zero tests passing, not crashing)

**Checkpoint**: Solution structure built; packages installed; fixtures in place; `dotnet test` exits cleanly.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domain model records that all user stories depend on.  
**⚠️ CRITICAL**: No user story implementation begins until models exist.

- [ ] T013 Create `src/DotNetTool.Core/Models/SolutionInfo.cs`: `public sealed record SolutionInfo(string FilePath, string Name, IReadOnlyList<ProjectInfo> Projects)`
- [ ] T014 [P] Create `src/DotNetTool.Core/Models/ProjectInfo.cs`: `public sealed record ProjectInfo(string Name, string ProjectType, string FilePath, string RelativePath, IReadOnlyList<NamespaceInfo> Namespaces, IReadOnlyList<string> LoadWarnings)`
- [ ] T015 [P] Create `src/DotNetTool.Core/Models/NamespaceInfo.cs`: `public sealed record NamespaceInfo(string FullName, IReadOnlyList<ClassInfo> Classes)`
- [ ] T016 [P] Create `src/DotNetTool.Core/Models/ClassInfo.cs`: `public sealed record ClassInfo(string Name, string FullyQualifiedName, bool IsNested, string? ContainingTypeName)`
- [ ] T017 Create `src/DotNetTool.Core/Output/IOutputFormatter.cs`: `public interface IOutputFormatter { string Format(SolutionInfo solution); }`
- [ ] T018 Verify `dotnet build` remains clean after model additions

**Checkpoint**: All four model records and `IOutputFormatter` compile. Foundation ready for US1.

---

## Phase 3: User Story 1 — Load Solution and List Projects (Priority: P1) 🎯 MVP

**Goal**: `dotnet-tool <path.sln>` prints all projects with names, types, and relative paths.

**Independent Test**: Invoke the tool with `SimpleSolution` fixture; verify 3 projects printed with correct types and paths; verify exit 1 for missing file path.

### Tests for User Story 1 ⚠️ Write FIRST — all must FAIL before implementation

- [ ] T019 [P] [US1] Write `tests/DotNetTool.Core.Tests/Analysis/SolutionLoaderTests.cs`: test that `SolutionLoader.LoadAsync` with `SimpleSolution` fixture returns a `SolutionInfo` with `Name == "SimpleSolution"` and `Projects.Count == 3`; test that passing a non-existent path throws `FileNotFoundException`
- [ ] T020 [P] [US1] Write `tests/DotNetTool.Core.Tests/Analysis/ProjectAnalyzerTests.cs`: test `GetProjectType` returns `"Console Application"` for `ConsoleApp`, `"Class Library"` for `CoreLib`, and `"Class Library"` for `EmptyLib` from the `SimpleSolution` fixture
- [ ] T021 [P] [US1] Write `tests/DotNetTool.Core.Tests/Output/TextOutputFormatterTests.cs` (projects-only phase): construct a `SolutionInfo` with two `ProjectInfo` records (no namespaces); assert the formatted string contains project names, type labels in `[brackets]`, and relative paths; assert stdout/stderr separation (warnings go to separate string)
- [ ] T022 Confirm `dotnet test` reports T019–T021 as failing (not erroring) — all three test classes must exist and their tests must fail with `NotImplementedException` or similar

### Implementation for User Story 1

- [ ] T023 [US1] Create `src/DotNetTool.Core/Analysis/SolutionLoader.cs`: implement `public static class SolutionLoader` with `public static async Task<SolutionInfo> LoadAsync(string solutionPath, IProgress<string>? diagnosticProgress = null, CancellationToken ct = default)`; call `MSBuildWorkspace.Create()` with `SkipUnrecognizedProjects = true`; call `OpenSolutionAsync`; surface `workspace.Diagnostics` via `diagnosticProgress`; throw `FileNotFoundException` for missing path; map each `Project` to `ProjectInfo` using `ProjectAnalyzer`; compute `RelativePath` with `Path.GetRelativePath`
- [ ] T024 [US1] Create `src/DotNetTool.Core/Analysis/ProjectAnalyzer.cs`: implement `public static class ProjectAnalyzer` with `public static string GetProjectType(Project project)` — check `MetadataReferences` for `Microsoft.AspNetCore` (→ `"Web Application"`); otherwise switch on `CSharpCompilationOptions.OutputKind`: `ConsoleApplication` → `"Console Application"`, `DynamicallyLinkedLibrary` → `"Class Library"`, `WindowsApplication` → `"Windows Application"`, default → `"Unknown"`
- [ ] T025 [US1] Create `src/DotNetTool.Core/Output/TextOutputFormatter.cs`: implement `IOutputFormatter`; render `Solution: <Name>  (<FilePath>)` header; for each project render `  <Name>  [<ProjectType>]\n    <RelativePath>`; for projects with empty `Namespaces` render `    (no public classes)`; for load warnings render `    [warn] <message>`
- [ ] T026 [US1] Create `src/DotNetTool.Cli/Program.cs`: `Bootstrap()` method calls `MSBuildLocator.RegisterDefaults()` as **first statement of `Main`**, before any other method call; define `Argument<FileInfo>("solution")` and root `RootCommand`; add `.sln` extension validation (exit 1 on failure); wire `SetAction` to call `SolutionLoader.LoadAsync`, then `TextOutputFormatter.Format`, write result to `Console.Out`; pipe `diagnosticProgress` to `Console.Error`; catch `FileNotFoundException` → exit 1; catch general `Exception` → exit 2
- [ ] T027 [US1] Run `dotnet test tests/DotNetTool.Core.Tests` — T019, T020, T021 must now pass; fix any regressions before proceeding

**Checkpoint**: `dotnet-tool SimpleSolution.sln` prints 3 projects. US1 acceptance scenarios pass. `dotnet test` green.

---

## Phase 4: User Story 2 — List Classes in Projects (Priority: P2)

**Goal**: Output groups all public classes by project and namespace, with fully qualified names; empty projects shown with `(no public classes)`.

**Independent Test**: Run the tool with `MultiNamespaceSolution` fixture; verify classes from 3 namespaces appear, private class excluded, empty project section shown.

### Tests for User Story 2 ⚠️ Write FIRST — all must FAIL before implementation

- [ ] T028 [P] [US2] Write `tests/DotNetTool.Core.Tests/Analysis/ClassExtractorTests.cs`: using `MultiNamespaceSolution` fixture compilation, assert `ClassExtractor.Extract` returns classes from `Alpha`, `Alpha.Sub`, and `Beta` namespaces; assert private class is NOT returned; assert a public nested class inside a public class IS returned with `IsNested == true` and `ContainingTypeName` set correctly
- [ ] T029 [P] [US2] Extend `tests/DotNetTool.Core.Tests/Output/TextOutputFormatterTests.cs` with namespace/class phase: construct `SolutionInfo` with a `ProjectInfo` containing 2 `NamespaceInfo` entries each with multiple `ClassInfo` records; assert the formatted output contains namespace names as section headers and fully qualified class names indented beneath; assert the `(no public classes)` line appears for a project with empty `Namespaces`
- [ ] T030 Confirm `dotnet test` reports T028–T029 tests as failing

### Implementation for User Story 2

- [ ] T031 [US2] Create `src/DotNetTool.Core/Analysis/ClassExtractor.cs`: implement `public static class ClassExtractor` with `public static IReadOnlyList<ClassInfo> Extract(Compilation compilation)` — recursively traverse `compilation.GlobalNamespace` via a stack of `INamespaceOrTypeSymbol`; yield `INamedTypeSymbol` where `TypeKind == TypeKind.Class` and `DeclaredAccessibility == Accessibility.Public`; recurse into nested types; populate `IsNested` and `ContainingTypeName` from `symbol.ContainingType`; use `symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)` for `FullyQualifiedName` (strip `global::` prefix)
- [ ] T032 [US2] Extend `src/DotNetTool.Core/Analysis/ProjectAnalyzer.cs`: add `public static async Task<IReadOnlyList<NamespaceInfo>> GetNamespacesAsync(Project project, CancellationToken ct)` — call `project.GetCompilationAsync(ct)`; call `ClassExtractor.Extract(compilation)`; group results by `symbol.ContainingNamespace.ToDisplayString()` (use `"(global)"` for empty namespace); return as sorted list of `NamespaceInfo`
- [ ] T033 [US2] Update `SolutionLoader.LoadAsync` in `src/DotNetTool.Core/Analysis/SolutionLoader.cs` to call `ProjectAnalyzer.GetNamespacesAsync` per project and populate `ProjectInfo.Namespaces`
- [ ] T034 [US2] Extend `src/DotNetTool.Core/Output/TextOutputFormatter.cs` to render namespaces and classes: after each project's path, render `    Namespaces:` header then for each `NamespaceInfo` render `      <FullName>` followed by `        <FullyQualifiedName>` per class; keep `(no public classes)` for empty `Namespaces`
- [ ] T035 [US2] Run `dotnet test` — T028, T029 must now pass and all prior tests remain green

**Checkpoint**: `dotnet-tool MultiNamespaceSolution.sln` shows classes grouped by namespace. US2 acceptance scenarios pass.

---

## Phase 5: User Story 3 — Export Solution Structure (Priority: P3)

**Goal**: `--format json` emits valid, complete JSON; `--format invalid` exits 1 with supported-formats list.

**Independent Test**: Run `dotnet-tool <fixture> --format json | jq .` — must parse without error and contain all projects and classes; run with `--format xyz` — must exit 1 with error message.

### Tests for User Story 3 ⚠️ Write FIRST — all must FAIL before implementation

- [ ] T036 [P] [US3] Write `tests/DotNetTool.Core.Tests/Output/JsonOutputFormatterTests.cs`: construct a `SolutionInfo` with 2 projects each with namespaces and classes; call `JsonOutputFormatter.Format`; parse result with `JsonDocument.Parse`; assert `projects.Length == 2`; assert first project `namespaces[0].classes[0].fullyQualifiedName` matches expected value; assert output is pretty-printed (contains newlines)
- [ ] T037 [P] [US3] Write integration test in `tests/DotNetTool.Integration.Tests/CliIntegrationTests.cs`: spawn `dotnet run --project src/DotNetTool.Cli -- <SimpleSolution-path> --format json` as a subprocess; assert exit code 0; assert stdout is valid JSON via `JsonDocument.Parse`; spawn with `--format invalid`; assert exit code 1; assert stderr contains `"Supported formats"`
- [ ] T038 Confirm `dotnet test` reports T036–T037 as failing

### Implementation for User Story 3

- [ ] T039 [US3] Create `src/DotNetTool.Core/Output/JsonOutputFormatter.cs`: implement `IOutputFormatter`; use `System.Text.Json.JsonSerializer.Serialize(solution, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase })` — ensure all record types are serialisable; add `[JsonPropertyName]` attributes where needed to match the JSON shape in `data-model.md`
- [ ] T040 [US3] Update `src/DotNetTool.Cli/Program.cs` to add `--format` option (default `"text"`, allowed `"text"` or `"json"`): validate format value after parsing; if invalid write to `Console.Error` `"Error: Unsupported format '<value>'. Supported formats: text, json"` and exit 1; select `TextOutputFormatter` or `JsonOutputFormatter` accordingly
- [ ] T041 [US3] Run `dotnet test` — T036, T037 must now pass and all prior tests remain green

**Checkpoint**: All three user stories independently functional. All tests green.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Performance validation, edge-case hardening, error message quality, packaging.

- [ ] T042 Add `--version` output to `src/DotNetTool.Cli/Program.cs` driven by `<Version>` in the `.csproj`; set initial version to `1.0.0`
- [ ] T043 [P] Add integration test in `tests/DotNetTool.Integration.Tests/CliIntegrationTests.cs` for missing-file scenario: spawn tool with non-existent `.sln` path; assert exit code 1; assert stderr contains `"Solution file not found"`
- [ ] T044 [P] Add integration test for non-`.sln` file: spawn tool with a `.txt` path; assert exit code 1; assert stderr contains `"Expected a .sln file"`
- [ ] T045 Add `coverlet.collector` configuration in `DotNetTool.sln` (or `Directory.Build.props`): configure minimum coverage threshold 90% for `DotNetTool.Core.Tests`
- [ ] T046 [P] Create `CHANGELOG.md` at repository root with initial `1.0.0` entry documenting US1, US2, US3 per Principle III
- [ ] T047 Run `dotnet pack src/DotNetTool.Cli` — verify `.nupkg` is produced in `src/DotNetTool.Cli/nupkg/`; install locally with `dotnet tool install --global --add-source src/DotNetTool.Cli/nupkg DotNetTool.Cli`; smoke-test with a real solution
- [ ] T048 Final `dotnet test --collect:"XPlat Code Coverage"` — all tests pass; coverage ≥ 90%; no regressions

**Checkpoint**: Tool is packaged, versioned, and all success criteria (SC-001 through SC-007) are met.

---

## Dependency Graph

```
T001 → T002 → T003 → T006
               ↓       ↓
              T004    T007
              T005
               ↓
       T008, T009, T010, T011
               ↓
              T012
               ↓
       T013, T014, T015, T016 (parallel)
               ↓
              T017
               ↓
              T018
               ↓
   T019, T020, T021 (parallel — write tests FIRST, must FAIL)
               ↓
              T022 (confirm failure)
               ↓
   T023, T024, T025, T026 (parallel where independent)
               ↓
              T027 (confirm green)
               ↓
   T028, T029 (parallel — write tests FIRST, must FAIL)
               ↓
              T030
               ↓
   T031, T032 (parallel), then T033
               ↓
          T034, T035
               ↓
   T036, T037 (parallel — write tests FIRST, must FAIL)
               ↓
              T038
               ↓
          T039, T040 (parallel)
               ↓
              T041
               ↓
   T042–T048 (mostly parallel polish)
```

## Parallel Execution by Story

| Story | Parallelisable task groups |
|-------|---------------------------|
| Setup | T004+T005, T007+T008+T009, T010+T011 |
| Foundation | T013+T014+T015+T016 |
| US1 tests | T019+T020+T021 |
| US1 impl | T023+T024+T025 (then T026 needs them) |
| US2 tests | T028+T029 |
| US2 impl | T031+T032 (then T033 needs them) |
| US3 tests | T036+T037 |
| US3 impl | T039+T040 |
| Polish | T043+T044+T045+T046 |

## Implementation Strategy

**MVP scope**: Complete Phases 1–3 (T001–T027). At that point `dotnet-tool <path.sln>` lists all projects — User Story 1 fully delivered.

**Increment 2**: Phase 4 (T028–T035). Class discovery added.

**Increment 3**: Phase 5 (T036–T041). JSON export and format selection added.

**Increment 4**: Phase 6 (T042–T048). Packaging, edge-case coverage, changelog.

## Task Count Summary

| Phase | Story | Tasks | Notes |
|-------|-------|-------|-------|
| Phase 1 Setup | — | T001–T012 (12) | Blocking infrastructure |
| Phase 2 Foundation | — | T013–T018 (6) | Model records |
| Phase 3 | US1 (P1) | T019–T027 (9) | 3 test + 1 confirm + 4 impl + 1 verify |
| Phase 4 | US2 (P2) | T028–T035 (8) | 2 test + 1 confirm + 4 impl + 1 verify |
| Phase 5 | US3 (P3) | T036–T041 (6) | 2 test + 1 confirm + 2 impl + 1 verify |
| Phase 6 Polish | — | T042–T048 (7) | Cross-cutting concerns |
| **Total** | | **48** | |

**Format validation**: All tasks follow `- [ ] T### [P?] [US?] Description with file path`. ✅
