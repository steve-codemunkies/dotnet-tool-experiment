# Data Model: Solution Loading and Structure Discovery

**Feature**: `001-solution-loading`  
**Date**: 2026-03-17

---

## Entity Overview

```
SolutionInfo
└── ProjectInfo[]
    └── NamespaceInfo[]
        └── ClassInfo[]
```

All entities are immutable value objects (C# `record` types). They represent the
output of Roslyn analysis — not EF/database models.

---

## Entities

### SolutionInfo

Represents the top-level .NET solution loaded from a `.sln` file.

| Field | Type | Description |
|-------|------|-------------|
| `FilePath` | `string` | Absolute path to the `.sln` file |
| `Name` | `string` | Solution name (filename without extension) |
| `Projects` | `IReadOnlyList<ProjectInfo>` | All projects discovered in the solution |

**Source**: `MSBuildWorkspace.OpenSolutionAsync()` → `Solution.FilePath`, `Solution.Projects`

**Validation rules**:
- `FilePath` must be non-null and exist on disk (validated before loading)
- `Projects` may be empty (valid: solution with no projects)

---

### ProjectInfo

Represents a single .NET project (`.csproj`) within the solution.

| Field | Type | Description |
|-------|------|-------------|
| `Name` | `string` | Project name (assembly name) |
| `ProjectType` | `string` | Human-readable type: `Console Application`, `Class Library`, `Web Application`, `Windows Application`, `Unknown` |
| `FilePath` | `string` | Absolute path to the `.csproj` file |
| `RelativePath` | `string` | Path relative to the solution directory |
| `Namespaces` | `IReadOnlyList<NamespaceInfo>` | Namespaces containing public classes (empty list if none) |
| `LoadWarnings` | `IReadOnlyList<string>` | Diagnostic messages if project had load issues |

**Source**: `Project` from Roslyn `Solution.Projects`; `CSharpCompilationOptions.OutputKind` for type; `MetadataReferences` for web detection.

**Validation rules**:
- `Namespaces` is always initialised (empty list, never null)
- `LoadWarnings` is always initialised; populated from `workspace.Diagnostics` after open

**State transitions**:
- Project can be in `Loaded` or `PartiallyLoaded` (has warnings) state; in both cases it appears in output

---

### NamespaceInfo

Represents a namespace that contains one or more public classes within a project.

| Field | Type | Description |
|-------|------|-------------|
| `FullName` | `string` | Fully qualified namespace (e.g., `MyApp.Core.Services`) |
| `Classes` | `IReadOnlyList<ClassInfo>` | Public classes within this namespace |

**Source**: `INamedTypeSymbol.ContainingNamespace.ToDisplayString()` — grouped after class extraction.

**Validation rules**:
- `Classes` is always non-empty (namespaces with no public classes are not emitted)
- Global namespace (`""`) is represented as `"(global)"` for display

---

### ClassInfo

Represents a single public class symbol extracted from a project's compilation.

| Field | Type | Description |
|-------|------|-------------|
| `Name` | `string` | Simple class name (e.g., `OrderService`) |
| `FullyQualifiedName` | `string` | Namespace + class name (e.g., `MyApp.Core.Services.OrderService`) |
| `IsNested` | `bool` | True if this class is declared inside another class |
| `ContainingTypeName` | `string?` | Simple name of the enclosing type if `IsNested` is true |

**Source**: `INamedTypeSymbol` where `TypeKind == TypeKind.Class` and `DeclaredAccessibility == Accessibility.Public`.

**Extraction filter**:
- `TypeKind.Class` only — interfaces, structs, enums, delegates excluded
- `Accessibility.Public` only — internal, protected, private excluded
- Nested public classes within public classes are included

---

## Relationships

```
SolutionInfo 1 ──── * ProjectInfo        (one solution, many projects)
ProjectInfo  1 ──── * NamespaceInfo      (one project, many namespaces)
NamespaceInfo 1 ─── * ClassInfo          (one namespace, many classes)
ClassInfo may reference ClassInfo        (IsNested → ContainingTypeName)
```

---

## C# Record Definitions (canonical shape)

```csharp
public sealed record SolutionInfo(
    string FilePath,
    string Name,
    IReadOnlyList<ProjectInfo> Projects);

public sealed record ProjectInfo(
    string Name,
    string ProjectType,
    string FilePath,
    string RelativePath,
    IReadOnlyList<NamespaceInfo> Namespaces,
    IReadOnlyList<string> LoadWarnings);

public sealed record NamespaceInfo(
    string FullName,
    IReadOnlyList<ClassInfo> Classes);

public sealed record ClassInfo(
    string Name,
    string FullyQualifiedName,
    bool IsNested,
    string? ContainingTypeName);
```

---

## JSON Shape (P3 export)

```json
{
  "name": "MyApp",
  "filePath": "/repos/MyApp/MyApp.sln",
  "projects": [
    {
      "name": "MyApp.Core",
      "projectType": "Class Library",
      "filePath": "/repos/MyApp/src/MyApp.Core/MyApp.Core.csproj",
      "relativePath": "src/MyApp.Core/MyApp.Core.csproj",
      "loadWarnings": [],
      "namespaces": [
        {
          "fullName": "MyApp.Core.Services",
          "classes": [
            {
              "name": "OrderService",
              "fullyQualifiedName": "MyApp.Core.Services.OrderService",
              "isNested": false,
              "containingTypeName": null
            }
          ]
        }
      ]
    }
  ]
}
```
