<!--
SYNC IMPACT REPORT — Constitution v1.0.0
=========================================

Version Change: None → 1.0.0 (Initial Constitution)
Rationale: New 3-principle constitution for dotnet-tool-experiment project focusing on CLI-driven 
development, test-first practices, and semantic versioning.

Modified Principles:
- N/A (Initial version)

Added Sections:
- Core Principles (3 principles)
- Quality Standards
- Governance section with amendment procedures

Removed Sections:
- N/A (Initial version)

Template Sync Status:
✅ plan-template.md: Constitution Check section references constitution correctly (no changes needed)
✅ spec-template.md: Feature specification structure aligns with principles (no changes needed)
✅ tasks-template.md: Test-first phases align with "Test-First Development" principle (no changes needed)
✅ checklist-template.md: Reference integration not applicable (no changes needed)
⚠️ agent-file-template.md: May reference constitution (verify during agent implementation)

Follow-up TODOs:
- None at this time; all placeholders replaced

Validation Complete:
✓ No unexplained bracket tokens remain
✓ Version: 1.0.0 (semantic versioning)
✓ Dates in ISO format: 2026-03-15
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

## Quality Standards

- Code must be peer-reviewed before merge; reviews verify compliance with the three core principles.
- Automated tests must pass and code coverage must not decrease.
- Documentation (README, guide, API docs) must be updated in tandem with code changes.

## Governance

This constitution supersedes all conflicting practices. Amendments require documentation of rationale,
affected principles, and migration plan. All PRs and reviews must verify explicit compliance with 
these principles. Use runtime guidance in `.github/agents/` and project documentation for detailed 
implementation patterns.

**Version**: 1.0.0 | **Ratified**: 2026-03-15 | **Last Amended**: 2026-03-15
