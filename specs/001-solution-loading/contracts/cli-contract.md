# CLI Contract: `dotnet-tool` Solution Loading

**Feature**: `001-solution-loading`  
**Date**: 2026-03-17  
**Tool name**: `dotnet-tool` (package ID: `DotNetTool.Cli`)

---

## Command Schema

```
dotnet-tool <solution> [--format <format>] [--help] [--version]
```

---

## Arguments

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `solution` | `FileInfo` (path) | **Yes** | Absolute or relative path to the `.NET` solution file (`.sln`) |

**Validation**:
- File must exist; if not → exit code `1`, stderr: `Error: Solution file not found: <path>`
- File must have `.sln` extension; if not → exit code `1`, stderr: `Error: Expected a .sln file, got: <path>`

---

## Options

| Option | Type | Default | Allowed Values | Description |
|--------|------|---------|----------------|-------------|
| `--format` | `string` | `text` | `text`, `json` | Output format |
| `--help`, `-h` | flag | — | — | Print usage and exit `0` |
| `--version` | flag | — | — | Print tool version (SemVer) and exit `0` |

**Format validation**:  
If `--format` value is not in the allowed set → exit code `1`, stderr:
```
Error: Unsupported format 'xyz'. Supported formats: text, json
```

---

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success — solution loaded and output written to stdout |
| `1` | User error — bad arguments, file not found, unsupported format |
| `2` | Analysis error — solution could not be loaded (MSBuild/Roslyn failure) |

---

## Standard Output (stdout)

All successful output goes to **stdout** only. Nothing else is written to stdout.

### `--format text` (default)

```
Solution: <name>  (<path>)

  <ProjectName>  [<ProjectType>]
    <path-relative-to-solution>

    Namespaces:
      <Namespace.One>
        <FullyQualifiedClassName>
        <FullyQualifiedClassName>
      <Namespace.Two>
        <FullyQualifiedClassName>

  <NextProjectName>  [<ProjectType>]
    ...
```

**Empty project** (no public classes):
```
  MyApp.Core  [Class Library]
    src/MyApp.Core/MyApp.Core.csproj

    (no public classes)
```

### `--format json`

Valid JSON matching the `SolutionInfo` schema defined in [data-model.md](../data-model.md).
Written as a single JSON object, pretty-printed with 2-space indentation.

---

## Standard Error (stderr)

Progress and warning messages go to **stderr** only, so stdout remains pipe-safe.

| Situation | stderr message |
|-----------|---------------|
| Project load warning | `[warn] <ProjectName>: <diagnostic message>` |
| Project skipped (unrecognised) | `[skip] <ProjectName>: unrecognised project format` |
| Solution load failure | `Error: Failed to open solution: <detail>` and exit `2` |

---

## Invocation Examples

```bash
# List projects and classes (text, default)
dotnet-tool MyApp.sln

# JSON output piped to jq
dotnet-tool MyApp.sln --format json | jq '.projects[].name'

# Absolute path
dotnet-tool /repos/MyApp/MyApp.sln --format json

# Handle missing file (exit 1)
dotnet-tool /does/not/exist.sln
# → stderr: Error: Solution file not found: /does/not/exist.sln
# → exit code: 1
```

---

## Versioning

The CLI contract is versioned with the tool package. Breaking changes (adding required
arguments, removing options, changing exit code semantics) require a **MAJOR** version
bump per Principle III (Semantic Versioning). Adding new optional options is a **MINOR**
bump.
