<!--
SYNC IMPACT REPORT — Constitution v1.1.0
=========================================

Version Change: 1.0.0 → 1.1.0
Bump Type: MINOR — new principle added (Roslyn-Powered .NET Analysis)
Rationale: A new non-negotiable technical constraint mandates the use of the Roslyn Compiler
Platform for all .NET solution/project/source analysis. Text-based and regex parsing is
explicitly prohibited. This overrides earlier design decisions (e.g., the regex-based approach
proposed in feature 001-solution-loading planning) and applies retroactively to all features.

Modified Principles:
- None modified (existing three principles unchanged)

Added Sections:
- Principle IV: Roslyn-Powered .NET Analysis

Removed Sections:
- None

Template Sync Status:
✅ plan-template.md: Constitution Check section is generic; no changes needed. Plans for
   .NET-related features MUST now cite Principle IV compliance explicitly.
✅ spec-template.md: Generic structure aligns with updated principles; no changes needed.
✅ tasks-template.md: Generic task structure unchanged; feature tasks MUST add Roslyn
   dependency setup (Microsoft.CodeAnalysis NuGet packages) as a foundational task.
✅ checklist-template.md: Reference integration not applicable; no changes needed.
⚠️ agent-file-template.md: Verify no references to regex/text-based parsing patterns remain.

Feature Impact:
⚠️ specs/001-solution-loading/spec.md: FR-004 assumes .csproj parsing; must be re-evaluated
   with Roslyn Workspace/Project APIs as the required implementation approach.
⚠️ specs/001-solution-loading/plan.md (if exists): Any "regex-based parsing" design decision
   is overridden by Principle IV and MUST be replaced with Roslyn APIs.

Follow-up TODOs:
- Update specs/001-solution-loading plan to reflect Roslyn-based implementation approach.
- Ensure NuGet package references (Microsoft.CodeAnalysis.Workspaces.MSBuild,
  Microsoft.Build.Locator) are added to foundational tasks in any generated tasks.md.

Validation Complete:
✓ No unexplained bracket tokens remain
✓ Version: 1.1.0 (semantic versioning)
✓ Dates in ISO format: 2026-03-15 (ratified), 2026-03-17 (amended)
✓ Principles are declarative and testable
✓ Governance rules clearly stated
-->

# dotnet tool experiment Constitution

## Core Principles

### I. CLI-Driven Design
All core functionality must be accessible and fully operable through command-line interfaces.
Features are designed CLI-first; UI/API layers are secondary. Command arguments, options, and 
output formats (text and JSON) must support both human operators and automation scripts.

### II. Test-First Development (NON-NEGOTIABLE)
Test-driven development is mandatory. Tests are written and approved BEFORE implementation begins.
The Red-Green-Refactor cycle is strictly enforced: tests fail → implementation → tests pass → refactor.
All public APIs require accompanying contract tests; integration tests validate cross-component workflows.

### III. Semantic Versioning
All releases follow Semantic Versioning (MAJOR.MINOR.PATCH). MAJOR version increments indicate
breaking changes; MINOR for new backward-compatible features; PATCH for bug fixes and clarifications.
All breaking changes require explicit changelog entries and migration guidance.

### IV. Roslyn-Powered .NET Analysis
All analysis of .NET solutions, projects, and source code MUST use the Roslyn Compiler Platform
(Microsoft.CodeAnalysis and related NuGet packages such as Microsoft.CodeAnalysis.Workspaces.MSBuild
and Microsoft.Build.Locator). Direct text-based or regex parsing of .sln, .csproj, or .cs files is
prohibited. Roslyn APIs provide semantic correctness, proper symbol resolution, and IDE-grade fidelity
that text parsing cannot reliably deliver. Any feature performing static analysis, class discovery,
or project structure inspection MUST open a Roslyn Workspace and operate on the semantic model.

## Quality Standards

- Code must be peer-reviewed before merge; reviews verify compliance with the three core principles.
- Automated tests must pass and code coverage must not decrease.
- Documentation (README, guide, API docs) must be updated in tandem with code changes.

## Governance

This constitution supersedes all conflicting practices. Amendments require documentation of rationale,
affected principles, and migration plan. All PRs and reviews must verify explicit compliance with 
these principles. Use runtime guidance in `.github/agents/` and project documentation for detailed 
implementation patterns.

**Version**: 1.1.0 | **Ratified**: 2026-03-15 | **Last Amended**: 2026-03-17
