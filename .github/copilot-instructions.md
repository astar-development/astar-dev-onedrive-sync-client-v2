# GitHub Copilot Instructions for AStar.Dev.OneDrive.Sync.Client

> 📋 **Quick Reference**: [`/docs/copilot/quick-reference.md`](/docs/copilot/quick-reference.md)
>
> 📚 **Detailed Guides**:
> - [Functional Programming Guide](/docs/copilot/functional-programming-guide.md)
> - [Common Scenarios](/docs/copilot/common-scenarios.md)
> - [Migration Guide](/docs/copilot/migration-guide.md)
> - [Blog Series](/docs/blogs/) — In-depth explanations

---

## Test-Driven Development (TDD) — MANDATORY

**TDD is non-negotiable. Every feature branch MUST demonstrate Red → Green → Refactor.**

1. Write failing test → Commit (RED)
2. Write minimum code to pass → Commit (GREEN)  
3. Refactor while keeping tests green → Commit (REFACTOR)

> ❌ **PRs without a prior failing-test commit will be rejected.**

---

## Architecture — Onion Architecture

**Strict dependency rules:**

```
UI  →  Application  →  Domain
Infrastructure  →  Application  →  Domain
```

| Layer | Dependencies | Additional References |
|---|---|---|
| Domain | None | `AStar.Dev.Functional.Extensions` |
| Application | Domain only | (inherits from Domain) |
| Infrastructure | Domain, Application | (inherits from Domain) |
| UI | Application only (never Infrastructure) | `AStar.Dev.Utilities`, `AStar.Dev.Source.Generators` |

**Cross-cutting:** All layers access `Result<T, TError>`, `Option<T>`, `[StrongId]`, `[AutoRegisterService]`

---

## Functional Programming — MANDATORY

> 📚 **Full Guide**: [`/docs/copilot/functional-programming-guide.md`](/docs/copilot/functional-programming-guide.md)

### Core Rules

1. **NEVER return `null`** → Use `Option<T>`
2. **NEVER throw exceptions for control flow** → Use `Result<T, TError>`
3. **ALWAYS use `[StrongId]`** for domain identifiers
4. **ALWAYS use `[AutoRegisterService]`** for DI registration

### Quick Syntax

```csharp
// Result - operations that can fail
public Result<User, string> GetUser(int id)
{
    return user ?? "User not found";  // Implicit conversion
}

// Option - values that may not exist
public Option<User> FindUser(int id)
{
    return _users.FirstOrDefault(u => u.Id == id).ToOption();
}

// Unit - success with no value
public Result<Unit, string> ValidateEmail(string email)
{
    if (string.IsNullOrWhiteSpace(email))
        return "Email required";
    return Unit.Value;
}

// Try - wrap exceptions
public Result<string, Exception> ReadFile(string path)
{
    return Try.Run(() => File.ReadAllText(path));
}
```

### Layer-Specific Error Types

| Layer | TError | Example |
|---|---|---|
| Domain | `string` or custom record | `Result<User, string>` |
| Application | `ErrorResponse` or `Exception` | `Result<OrderDto, ErrorResponse>` |
| Infrastructure | `Exception` or `string` | `Result<Data, Exception>` |
| UI | `string` | `Result<Unit, string>` |

---

## Source Generators — MANDATORY

### StrongId

```csharp
[StrongId]  // Defaults to Guid
public readonly partial record struct UserId;

[StrongId(typeof(int))]
public readonly partial record struct OrderId;
```

**Benefits:** Type safety, no accidental ID mixing, zero overhead

### Service Registration

```csharp
[AutoRegisterService(ServiceLifetime.Scoped)]
public class UserService : IUserService { }

// In Program.cs:
builder.Services.AddGeneratedServices();
```

---

## Naming & Coding Standards

**Microsoft C# conventions + modern C# 14 features:**

- `PascalCase` — types, methods, properties
- `_camelCase` — private fields
- `IPascalCase` — interfaces
- Pattern matching
- Async methods: `MethodNameAsync` (production), optional in tests
- Tests: `MethodName_Scenario_ExpectedResult`
- Primary constructors, collection expressions `[]`, pattern matching
- File-scoped namespaces, expression-bodied members
- Use auto-properties and `field` for backing fields

### Mandates

- **NO comments in private members** — refactor until code is self-documenting
- **XML docs on all public/internal members**
- **Methods ≤ 20 lines** — refactor if longer
- **Classes ≤ 200 lines** — split if longer  
- **Parameters ≤ 5** — use parameter object if more
- **Immutability preferred** — mutable only when necessary (UI, performance)
- **Map/Bind/Match over if-else** — functional composition preferred

---

## Testing Stack

- **xUnit v3** — test framework
- **Shouldly** — fluent assertions (no `Assert.*`)
- **NSubstitute** — mocking

### Test Rules

- AAA pattern (Arrange, Act, Assert) — no comments marking sections
- Use `TestContext.Current.CancellationToken` for async
- Tests ≤ 10 lines — refactor code under test if longer
- NO comments in tests — refactor until clear

---

## UI Technology

- **AvaloniaUI** — cross-platform UI
- **ReactiveUI** — MVVM framework
- Views: `ReactiveWindow<TViewModel>` or `ReactiveUserControl<TViewModel>`
- ViewModels: inherit from `ViewModelBase` (extends `ReactiveObject`)
- Structure: `Views/` and `ViewModels/` with sub-folders by feature

---

## Branch Strategy

> 📄 See [Implementation Plan](../docs/implementation-plan.md) for the phased delivery plan.

| Branch | Purpose |
|---|---|
| `main` | Production-ready, protected |
| `feature/*` | Feature development with TDD commits |
| `copilot/*` | AI-generated branches |

> Each feature branch **must** show Red → Green → Refactor in commit history.

---

## Additional Resources

### Quick Lookups
- **Quick Reference**: [`/docs/copilot/quick-reference.md`](/docs/copilot/quick-reference.md) — Cheat sheets & lookup tables
- **Common Scenarios**: [`/docs/copilot/common-scenarios.md`](/docs/copilot/common-scenarios.md) — OneDrive-specific examples
- **Migration Guide**: [`/docs/copilot/migration-guide.md`](/docs/copilot/migration-guide.md) — Convert existing code

### In-Depth Learning
- **Functional Programming Guide**: [`/docs/copilot/functional-programming-guide.md`](/docs/copilot/functional-programming-guide.md) — Comprehensive patterns
- **Blog Series**: [`/docs/blogs/`](/docs/blogs/) — Theory & detailed explanations
  - [01: Overview](/docs/blogs/01-functional-extensions-overview.md)
  - [02: Result Deep Dive](/docs/blogs/02-result-type-deep-dive.md)
  - [03: Option Deep Dive](/docs/blogs/03-option-type-deep-dive.md)
  - [04: Supporting Types](/docs/blogs/04-supporting-types-and-extensions.md)
