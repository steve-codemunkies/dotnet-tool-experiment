# Changelog

All notable changes to this project will be documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] — 2026-03-17

### Added

#### User Story 1 — Load Solution and List Projects (P1 MVP)
- `dotnet-tool <path.sln>` opens a `.sln` file via Roslyn `MSBuildWorkspace` and prints all
  contained projects with their names, types (`Console Application`, `Class Library`,
  `Web Application`, `Windows Application`), and solution-relative paths.
- Exit code 1 on missing solution file; exit code 1 on non-`.sln` argument.
- Diagnostic warnings from MSBuild workspace surfaced to `stderr`.

#### User Story 2 — List Public Classes by Namespace (P2)
- Output groups all public classes by namespace under each project section.
- Private classes excluded; nested public classes included with `IsNested` flag and
  `ContainingTypeName` populated.
- Projects with no public classes show `(no public classes)`.

#### User Story 3 — Export Solution Structure as JSON (P3)
- `--format json` flag emits fully-structured JSON (pretty-printed, camelCase keys).
- `--format invalid` exits 1 with `"Supported formats: text, json"` on stderr.
- Default format remains `text`.

#### Infrastructure
- `.NET 10` global CLI tool packaged as `DotNetTool.Cli` (`dotnet-tool` command).
- Roslyn `MSBuildWorkspace` with `Microsoft.Build.Locator.RegisterDefaults()` for
  cross-platform MSBuild discovery.
- `System.CommandLine` 2.0.0-beta4 for CLI argument parsing.
- xUnit + FluentAssertions + coverlet for unit and integration testing.
