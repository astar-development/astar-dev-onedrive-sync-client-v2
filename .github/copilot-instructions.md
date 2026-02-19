# GitHub Copilot Instructions for AstarOneDrive

## Test-Driven Development (TDD) — MANDATORY

TDD is **not optional** — it is the **only** accepted workflow in this repository.

### TDD Workflow
1. **Write a failing test first.** No production code may be written without a corresponding failing test committed to the feature branch.
2. **Commit the failing test** to the feature branch before writing any production code.
3. **Write the minimum production code** required to make the failing test pass.
4. **Refactor** while keeping all tests green.

> ❌ **Pull requests that introduce production code without a prior failing-test commit will be rejected.**

---

## Architecture — Onion Architecture

This solution follows **Onion Architecture** with strict dependency rules:

```
UI  →  Application  →  Domain
Infrastructure  →  Application  →  Domain
```

| Layer | Project | Allowed Dependencies |
|---|---|---|
| Domain | `AstarOneDrive.Domain` | None (no external project references) |
| Application | `AstarOneDrive.Application` | Domain only |
| Infrastructure | `AstarOneDrive.Infrastructure` | Domain, Application |
| UI | `AstarOneDrive.UI` | Application only (not Infrastructure) |

> ❌ **The UI layer must never reference the Infrastructure layer directly.**
> Dependency injection (composition root) is the only mechanism for wiring Infrastructure implementations.

---

## Naming and Coding Standards

**Microsoft C# Naming Conventions MUST be followed at all times:**

- `PascalCase` — types, methods, properties, events, public fields
- `camelCase` with `_` prefix — private fields (e.g., `_myField`)
- `IPascalCase` — interfaces (e.g., `IUserRepository`)
- `PascalCase` — constants and static readonly fields
- Async methods **must** have the `Async` suffix (e.g., `GetFilesAsync`)
- Test method names: `MethodName_Scenario_ExpectedResult` pattern

**All warnings are treated as errors** (`TreatWarningsAsErrors=true` in `Directory.Build.props`).
> ❌ No warnings may be introduced or suppressed without explicit justification.

---

## C# Language Version

This project uses **C# 14** (LangVersion=preview with .NET 10). You **must** use modern C# features where appropriate:

- Primary constructors for simple types
- Collection expressions (e.g., `[]` instead of `Array.Empty<T>()`)
- Pattern matching
- `required` members
- `init`-only setters
- Null-coalescing assignment (`??=`)
- `ArgumentException.ThrowIfNullOrWhiteSpace`, `ArgumentNullException.ThrowIfNull`
- Target-typed `new` expressions
- File-scoped namespaces (one namespace per file)
- Top-level statements where applicable

> ❌ Do **not** use obsolete C# patterns when modern equivalents exist.

---

## Testing Stack

All test projects use:

| Tool | Purpose |
|---|---|
| **XUnit.V3** (`xunit.v3`) | Test framework |
| **Shouldly** | Fluent assertions |
| **NSubstitute** | Mocking / substitution |

### Rules
- Every new class or interface **must** have corresponding tests.
- Use `NSubstitute` for mocking dependencies — no hand-rolled fakes.
- Use `Shouldly` for all assertions — no raw `Assert.*` calls.
- Use `TestContext.Current.CancellationToken` when calling `async` methods that accept `CancellationToken`.

---

## UI Technology

- **AvaloniaUI** — cross-platform UI framework
- **ReactiveUI** — MVVM reactive framework (`Avalonia.ReactiveUI`)
- All views live in `src/AstarOneDrive.UI/Views/`
- All view models live in `src/AstarOneDrive.UI/ViewModels/`
- Use sub-folders for each UI area (e.g., `Views/Sync/`, `ViewModels/Sync/`)
- Views inherit from `ReactiveWindow<TViewModel>` or `ReactiveUserControl<TViewModel>`
- View models inherit from `ViewModelBase` (which extends `ReactiveObject`)

---

## Branch Strategy

| Branch | Purpose |
|---|---|
| `main` | Production-ready code only |
| `feature/*` | Feature branches — TDD commits required |
| `copilot/*` | Copilot-generated feature branches |

> Each feature branch **must** demonstrate the Red → Green → Refactor cycle via its commit history.
