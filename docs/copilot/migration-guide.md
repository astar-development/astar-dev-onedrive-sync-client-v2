# Migration Guide

This guide helps you migrate existing code to use functional programming patterns with `Result<T, TError>` and `Option<T>`.

---

## Migration Checklist

### Phase 1: Low-Hanging Fruit
- [ ] Replace all `null` returns with `Option<T>`
- [ ] Replace `.FirstOrDefault()` with `.FirstOrDefault().ToOption()`
- [ ] Replace nullable reference/value types with `Option<T>` in public APIs
- [ ] Add `[StrongId]` to all ID types
- [ ] Add `[AutoRegisterService]` to all service classes

### Phase 2: Exception Handling
- [ ] Identify all methods that throw exceptions for control flow
- [ ] Wrap exception-prone external calls with `Try.Run()` or `Try.RunAsync()`
- [ ] Replace exception-based validation with `Result<Unit, string>`
- [ ] Convert API error responses to `Result<T, ErrorResponse>`

### Phase 3: Composition
- [ ] Chain related operations using `.Map()` and `.Bind()`
- [ ] Remove intermediate variables where chaining is clearer
- [ ] Use `Match` instead of if-else on Result/Option

### Phase 4: Testing
- [ ] Update tests to assert on Result/Option states
- [ ] Remove `Assert.Throws<T>()` patterns
- [ ] Use `Match` in test assertions

---

## Before & After Examples

### Example 1: Null Returns → Option<T>

#### ❌ Before
```csharp
public User FindUserByEmail(string email)
{
    var user = _users.FirstOrDefault(u => u.Email == email);
    if (user == null)
        return null;
    
    return user;
}

// Usage
var user = FindUserByEmail("test@example.com");
if (user != null)  // Easy to forget!
{
    Console.WriteLine($"Found: {user.Name}");
}
```

#### ✅ After
```csharp
public Option<User> FindUserByEmail(string email)
{
    return _users.FirstOrDefault(u => u.Email == email).ToOption();
}

// Usage
var userOption = FindUserByEmail("test@example.com");
userOption.Match(
    onSome: user => Console.WriteLine($"Found: {user.Name}"),
    onNone: () => Console.WriteLine("User not found")
);
```

---

### Example 2: Exception Throwing → Result<T, TError>

#### ❌ Before
```csharp
public User GetUser(int id)
{
    if (id <= 0)
        throw new ArgumentException("Invalid user ID");
    
    var user = _repository.GetUser(id);
    if (user == null)
        throw new UserNotFoundException($"User {id} not found");
    
    return user;
}

// Usage
try
{
    var user = GetUser(42);
    Console.WriteLine($"Hello, {user.Name}!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
}
catch (UserNotFoundException ex)
{
    Console.WriteLine($"Not found: {ex.Message}");
}
```

#### ✅ After
```csharp
public Result<User, string> GetUser(int id)
{
    if (id <= 0)
        return "Invalid user ID";
    
    return _repository.FindUser(id)
        .ToResult(() => $"User {id} not found");
}

// Usage
var result = GetUser(42);
result.Match(
    onSuccess: user => Console.WriteLine($"Hello, {user.Name}!"),
    onFailure: error => Console.WriteLine($"Error: {error}")
);
```

---

### Example 3: Try-Catch → Try.Run()

#### ❌ Before
```csharp
public string ReadConfigFile(string path)
{
    try
    {
        return File.ReadAllText(path);
    }
    catch (FileNotFoundException ex)
    {
        _logger.LogError(ex, "Config file not found");
        return null;  // 💥 Null again!
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to read config");
        throw;  // Or should we return null? Inconsistent!
    }
}
```

#### ✅ After
```csharp
public Result<string, Exception> ReadConfigFile(string path)
{
    return Try.Run(() => File.ReadAllText(path))
        .TapError(ex => _logger.LogError(ex, "Failed to read config file"));
}

// Usage
var result = ReadConfigFile("config.json");
result.Match(
    onSuccess: content => ProcessConfig(content),
    onFailure: ex => UseDefaultConfig()
);
```

---

### Example 4: Nested Null Checks → Chaining

#### ❌ Before
```csharp
public decimal? CalculateShipping(int userId)
{
    var user = _userRepository.GetUser(userId);
    if (user == null)
        return null;
    
    var address = user.ShippingAddress;
    if (address == null)
        return null;
    
    var zipCode = address.ZipCode;
    if (string.IsNullOrEmpty(zipCode))
        return null;
    
    var rate = _shippingService.GetRate(zipCode);
    return rate;
}
```

#### ✅ After
```csharp
public Option<decimal> CalculateShipping(int userId)
{
    return _userRepository.FindUser(userId)        // Option<User>
        .Map(user => user.ShippingAddress)         // Option<Address>
        .Map(address => address.ZipCode)           // Option<string>
        .Bind(zipCode => _shippingService.GetRate(zipCode)); // Option<decimal>
}
```

---

### Example 5: Primitive IDs → StrongId

#### ❌ Before
```csharp
public class User
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
}

// Easy to mix up!
var order = GetOrder(user.OrganizationId);  // 💥 Wrong ID!
```

#### ✅ After
```csharp
[StrongId(typeof(int))]
public readonly partial record struct UserId;

[StrongId(typeof(int))]
public readonly partial record struct OrderId;

[StrongId(typeof(int))]
public readonly partial record struct OrganizationId;

public class User
{
    public UserId Id { get; set; }
    public OrganizationId OrganizationId { get; set; }
}

public class Order
{
    public OrderId Id { get; set; }
    public UserId UserId { get; set; }
}

// Compile error - type safety!
var order = GetOrder(user.OrganizationId);  // ❌ Compile error: cannot convert OrganizationId to OrderId
```

---

### Example 6: Manual DI Registration → AutoRegisterService

#### ❌ Before
```csharp
// Program.cs
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddTransient<IEmailService, EmailService>();
// ... 50 more lines of manual registration

// Easy to forget to register!
public class NewFeatureService : INewFeatureService  
{
    // Oops, forgot to add to Program.cs
}
```

#### ✅ After
```csharp
// Services
[AutoRegisterService(ServiceLifetime.Scoped)]
public class UserService : IUserService { }

[AutoRegisterService(ServiceLifetime.Scoped)]
public class OrderService : IOrderService { }

[AutoRegisterService(ServiceLifetime.Singleton)]
public class CacheService : ICacheService { }

[AutoRegisterService(ServiceLifetime.Transient)]
public class EmailService : IEmailService { }

[AutoRegisterService(ServiceLifetime.Scoped)]
public class NewFeatureService : INewFeatureService { }  // Automatically registered!

// Program.cs - one line!
builder.Services.AddGeneratedServices();
```

---

### Example 7: Complex Validation → Result Chaining

#### ❌ Before
```csharp
public bool ValidateOrder(Order order, out string errorMessage)
{
    errorMessage = null;
    
    if (order == null)
    {
        errorMessage = "Order cannot be null";
        return false;
    }
    
    if (order.Items == null || order.Items.Count == 0)
    {
        errorMessage = "Order must contain at least one item";
        return false;
    }
    
    if (order.TotalAmount <= 0)
    {
        errorMessage = "Order total must be greater than zero";
        return false;
    }
    
    if (string.IsNullOrWhiteSpace(order.CustomerEmail))
    {
        errorMessage = "Customer email is required";
        return false;
    }
    
    return true;
}

// Usage
if (!ValidateOrder(order, out var error))
{
    throw new ValidationException(error);  // Exception again!
}
```

#### ✅ After
```csharp
public Result<Unit, string> ValidateOrder(Order order)
{
    if (order == null)
        return "Order cannot be null";
    
    if (order.Items == null || order.Items.Count == 0)
        return "Order must contain at least one item";
    
    if (order.TotalAmount <= 0)
        return "Order total must be greater than zero";
    
    if (string.IsNullOrWhiteSpace(order.CustomerEmail))
        return "Customer email is required";
    
    return Unit.Value;
}

// Usage
var validationResult = ValidateOrder(order);
validationResult.Match(
    onSuccess: _ => ProcessOrder(order),
    onFailure: error => ShowValidationError(error)
);
```

---

## Testing Migration

### Before: Exception-Based Tests

```csharp
[Fact]
public void GetUser_InvalidId_ThrowsArgumentException()
{
    Assert.Throws<ArgumentException>(() => _service.GetUser(-1));
}

[Fact]
public void GetUser_NotFound_ThrowsUserNotFoundException()
{
    Assert.Throws<UserNotFoundException>(() => _service.GetUser(999));
}
```

### After: Result-Based Tests

```csharp
[Fact]
public void GetUser_InvalidId_ReturnsError()
{
    var result = _service.GetUser(-1);
    
    result.Match(
        onSuccess: _ => throw new Exception("Expected error"),
        onFailure: error => error.ShouldContain("Invalid")
    );
}

[Fact]
public void GetUser_NotFound_ReturnsError()
{
    var result = _service.GetUser(999);
    
    result.Match(
        onSuccess: _ => throw new Exception("Expected error"),
        onFailure: error => error.ShouldContain("not found")
    );
}
```

---

## Common Pitfalls

### Pitfall 1: Forgetting to Handle Both Cases

```csharp
// ❌ Only handling success - what about None?
var email = FindUser(userId)
    .Map(user => user.Email);
// email is Option<string> - might be None!

// ✅ Always use Match to handle both cases
var message = FindUser(userId)
    .Map(user => user.Email)
    .Match(
        onSome: email => $"Email: {email}",
        onNone: () => "No email found"
    );
```

### Pitfall 2: Nesting Results/Options

```csharp
// ❌ Nested Result
var result = GetUser(userId)
    .Map(user => GetOrder(user.Id));  // Result<Result<Order, string>, string> 💥

// ✅ Use Bind for operations that return Result
var result = GetUser(userId)
    .Bind(user => GetOrder(user.Id));  // Result<Order, string> ✅
```

### Pitfall 3: Mixing Null and Option

```csharp
// ❌ Don't mix null and Option
public Option<User> FindUser(int id)
{
    var user = _repo.GetUser(id);  // Returns User or null
    if (user == null)
        return Option.None<User>();
    return Option.Some(user);
}

// ✅ Use ToOption
public Option<User> FindUser(int id)
{
    return _repo.GetUser(id).ToOption();
}
```

---

## Incremental Migration Strategy

### Step 1: Start with New Code
- All new features use Result/Option from day one
- Serves as examples for team members
  
### Step 2: Migrate Public APIs
- Start with public-facing APIs (controllers, services)
- High visibility, high value
- Update tests alongside

### Step 3: Migrate Core Domain
- Update domain entities and value objects
- Add StrongId types for identifiers
- Update domain services

### Step 4: Migrate Infrastructure
- Wrap external calls with Try
- Return Result from repositories
- Handle exceptions at boundaries

### Step 5: Clean Up Tests
- Remove Assert.Throws patterns
- Use Match for assertions
- Ensure all test types are covered

---

## Resources

- **Quick Reference**: [`/docs/copilot/quick-reference.md`](/docs/copilot/quick-reference.md)
- **Functional Programming Guide**: [`/docs/copilot/functional-programming-guide.md`](/docs/copilot/functional-programming-guide.md)
- **Common Scenarios**: [`/docs/copilot/common-scenarios.md`](/docs/copilot/common-scenarios.md)
- **Blog Series**: [`/docs/blogs/`](/docs/blogs/)
