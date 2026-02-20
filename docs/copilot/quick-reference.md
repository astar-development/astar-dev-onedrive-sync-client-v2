# Quick Reference Guide

## When to Use What

| Scenario | Use This | Example |
|---|---|---|
| Method can fail | `Result<T, TError>` | `Result<User, string> GetUser(int id)` |
| Value may not exist | `Option<T>` | `Option<User> FindUser(int id)` |
| Success with no value | `Result<Unit, TError>` | `Result<Unit, string> ValidateEmail(string email)` |
| Wrap exceptions | `Try.Run()` | `Try.Run(() => File.ReadAllText(path))` |
| Domain identifier | `[StrongId]` | `[StrongId] public readonly partial record struct UserId;` |
| DI registration | `[AutoRegisterService]` | `[AutoRegisterService(ServiceLifetime.Scoped)]` |

---

## Result<T, TError> Quick Reference

### Creation

```csharp
// Implicit conversion
return user;                    // Ok
return "User not found";        // Error

// Try wrapper
return Try.Run(() => File.ReadAllText(path));

// Async with Option
return user.ToOption().ToResult(() => "Not found");
```

### Consumption

```csharp
// Pattern matching
result.Match(
    onSuccess: user => $"Hello, {user.Name}!",
    onFailure: error => $"Error: {error}"
);

// Chaining
result.Map(x => x * 2)               // Transform success
      .Bind(x => GetNext(x))         // Chain Result-returning
      .MapFailure(ex => ex.Message)  // Transform error
      .Tap(x => Log(x));             // Side effect
```

---

## Option<T> Quick Reference

### Creation

```csharp
user.ToOption()                 // Null check automatic
value.ToOption(v => v > 0)      // Conditional
Option.Some(value)              // Explicit Some
Option.None<T>()                // Explicit None
```

### Consumption

```csharp
// Pattern matching
option.Match(
    onSome: value => $"Found: {value}",
    onNone: () => "Not found"
);

// Chaining
option.Map(x => x.Email)             // Transform value
      .Bind(email => ValidateEmail(email))
      .ToResult(() => "Validation failed");

// Checking
if (option.IsSome()) { ... }
if (option.IsNone()) { ... }
option.TryGetValue(out var value)
```

---

## Extension Method Cheat Sheet

| Method | Result | Option | Purpose |
|---|---|---|---|
| `.Map()` | ✅ | ✅ | Transform success/some value |
| `.Bind()` | ✅ | ✅ | Chain operations returning Result/Option |
| `.Match()` | ✅ | ✅ | Pattern match and return value |
| `.MapFailure()` | ✅ | ❌ | Transform error value |
| `.Tap()` | ✅ | ✅ | Side effect without changing value |
| `.TapError()` | ✅ | ❌ | Side effect on error |
| `.ToOption()` | ❌ | N/A | Convert nullable to Option |
| `.ToResult()` | N/A | ✅ | Convert Option to Result |
| `.*Async()` | ✅ | ✅ | Async variant of any method |

---

## Layer-Specific Error Types

| Layer | Preferred TError | Example |
|---|---|---|
| **Domain** | `string` or custom error record | `Result<User, string>` |
| **Application** | `ErrorResponse` or `Exception` | `Result<OrderDto, ErrorResponse>` |
| **Infrastructure** | `Exception` or `string` | `Result<Data, Exception>` |
| **UI** | `string` | `Result<Unit, string>` |

---

## StrongId Patterns

```csharp
// Default (Guid)
[StrongId]
public readonly partial record struct UserId;

// Custom type
[StrongId(typeof(int))]
public readonly partial record struct OrderId;

[StrongId(typeof(string))]
public readonly partial record struct CustomerId;

// Usage - type-safe!
UserId userId = UserId.From(Guid.NewGuid());
OrderId orderId = OrderId.From(42);
// userId = orderId; // Compile error! ✅
```

---

## Service Registration Patterns

```csharp
// Basic registration
[AutoRegisterService(ServiceLifetime.Scoped)]
public class UserService : IUserService { }
// → Registers IUserService -> UserService as Scoped

// Self registration
[AutoRegisterService(ServiceLifetime.Singleton, AsSelf = true)]
public class CacheManager : ICacheManager { }
// → Registers both ICacheManager and CacheManager

// Override interface
[AutoRegisterService(ServiceLifetime.Transient, As = typeof(ISpecialService))]
public class MyService : IServiceA, ISpecialService { }
// → Registers ISpecialService -> MyService
```

---

## Common Test Patterns

```csharp
// Test Result success
result.Match(
    onSuccess: user => user.Id.ShouldBe(42),
    onFailure: _ => throw new Exception("Expected success")
);

// Test Result failure
result.Match(
    onSuccess: _ => throw new Exception("Expected failure"),
    onFailure: error => error.ShouldContain("Invalid")
);

// Test Option Some
option.IsSome().ShouldBeTrue();
option.Match(
    onSome: user => user.Id.ShouldBe(42),
    onNone: () => throw new Exception("Expected Some")
);

// Test Option None
option.IsNone().ShouldBeTrue();
```

---

## Anti-Patterns (NEVER Do This)

```csharp
// ❌ Returning null
public User FindUser(int id) => null;

// ❌ Throwing exceptions for control flow
public User GetUser(int id)
{
    if (id <= 0) throw new ArgumentException("Invalid ID");
    // ...
}

// ❌ Using try-catch for business logic
try {
    return _processor.Process(order);
} catch (Exception ex) {
    return "Failed";
}

// ❌ Verbose Option creation
public Option<User> GetUser(int id)
{
    var user = _repo.FindUser(id);
    if (user == null) return Option.None<User>();
    return Option.Some(user);
}
// Use: return _repo.FindUser(id).ToOption();

// ❌ Primitive IDs
public class User 
{
    public int Id { get; set; }  // Use [StrongId] instead!
}
```

---

## Resource Links

- **Detailed FP Guide**: [`/docs/copilot/functional-programming-guide.md`](/docs/copilot/functional-programming-guide.md)
- **Blog Series**: [`/docs/blogs/`](/docs/blogs/)
  - [Overview](/docs/blogs/01-functional-extensions-overview.md)
  - [Result Deep Dive](/docs/blogs/02-result-type-deep-dive.md)
  - [Option Deep Dive](/docs/blogs/03-option-type-deep-dive.md)
  - [Supporting Types](/docs/blogs/04-supporting-types-and-extensions.md)
- **Common Scenarios**: [`/docs/copilot/common-scenarios.md`](/docs/copilot/common-scenarios.md)
- **Migration Guide**: [`/docs/copilot/migration-guide.md`](/docs/copilot/migration-guide.md)
