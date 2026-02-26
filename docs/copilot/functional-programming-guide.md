# Functional Programming Guide

This guide provides comprehensive examples and patterns for using `Result<T, TError>` and `Option<T>` in the AStar.Dev.OneDrive.Sync.Client project.

> 📚 **For detailed explanations and theory**, see the [blog series](/docs/blogs/).

---

## Result<T, TError> — Comprehensive Examples

### Layer-Specific Error Types

#### Domain Layer — Simple String Errors

```csharp
namespace AStar.Dev.OneDrive.Sync.Client.Domain.Entities;

public class SyncFile
{
    public Result<Unit, string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Path))
            return "Path cannot be empty";
        
        if (Size < 0)
            return "Size cannot be negative";
        
        return Unit.Value;
    }
}
```

#### Application Layer — ErrorResponse for Business Logic

```csharp
namespace AStar.Dev.OneDrive.Sync.Client.Application.Services;

public class SyncService : ISyncService
{
    public async Task<Result<SyncResult, ErrorResponse>> SyncFileAsync(
        string localPath, 
        string remotePath,
        CancellationToken ct)
    {
        return await ValidateLocalPath(localPath)
            .ToResult(() => new ErrorResponse("InvalidPath", "Local path does not exist"))
            .BindAsync(async path => await ReadFileAsync(path, ct))
            .BindAsync(async content => await UploadToOneDriveAsync(remotePath, content, ct))
            .MapFailure(ex => new ErrorResponse("SyncFailed", $"Sync failed: {ex.Message}"));
    }
}
```

#### Infrastructure Layer — Exception for External Services

```csharp
namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Repositories;

public class OneDriveSyncFileRepository : ISyncFileRepository
{
    public async Task<Result<SyncFile, Exception>> GetFileAsync(string path, CancellationToken ct)
    {
        return await Try.RunAsync(async () =>
        {
            var response = await _httpClient.GetAsync($"files/{path}", ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SyncFile>(ct);
        });
    }
}
```

#### UI Layer — User-Friendly Strings

```csharp
namespace AStar.Dev.OneDrive.Sync.Client.UI.ViewModels;

public class SyncViewModel : ViewModelBase
{
    public async Task<Result<Unit, string>> SyncAllFilesAsync(CancellationToken ct)
    {
        var result = await _syncService.SyncAllAsync(ct);
        
        return result.MapFailure(error => 
            error switch
            {
                { Code: "NetworkError" } => "Unable to connect to OneDrive. Check your internet connection.",
                { Code: "AuthError" } => "Authentication failed. Please sign in again.",
                { Code: "QuotaExceeded" } => "OneDrive storage quota exceeded.",
                _ => $"Sync failed: {error.Message}"
            }
        );
    }
}
```

---

## Chaining Patterns

### Simple Map Chain

```csharp
public Result<decimal, string> CalculateDiscountedPrice(string productId)
{
    return GetProduct(productId)              // Result<Product, string>
        .Map(product => product.Price)         // Result<decimal, string>
        .Map(price => price * 0.9m)            // Apply 10% discount
        .Map(price => price * 1.08m);          // Add 8% tax
}
```

### Bind Chain (Multiple Result-Returning Operations)

```csharp
public async Task<Result<OrderTotal, string>> GetOrderTotalAsync(int userId, CancellationToken ct)
{
    return await GetUserAsync(userId, ct)                    // Result<User, string>
        .BindAsync(async user => await GetLatestOrderAsync(user.Id, ct))  // Result<Order, string>
        .BindAsync(async order => await CalculateTotalAsync(order, ct));  // Result<OrderTotal, string>
}
```

### Mixed Map and Bind

```csharp
public async Task<Result<string, Exception>> ProcessUserDataAsync(int userId, CancellationToken ct)
{
    return await Try.RunAsync(async () => await _repo.GetUserAsync(userId, ct))
        .MapAsync(async user => await EnrichUserData(user))
        .BindAsync(async enriched => await ValidateAsync(enriched, ct))
        .MapAsync(async validated => await FormatForDisplay(validated))
        .MapFailure(ex => new Exception($"Processing failed: {ex.Message}"));
}
```

---

## Option<T> — Comprehensive Examples

### Creating Options

```csharp
// From nullable reference
public Option<User> FindUserByEmail(string email)
{
    var user = _users.FirstOrDefault(u => u.Email == email);
    return user.ToOption();
}

// With condition
public Option<int> GetPositiveNumber(int value)
{
    return value.ToOption(v => v > 0);
}

// From nullable value type
public Option<DateTime> GetLastSyncTime()
{
    DateTime? lastSync = _settings.LastSyncTime;
    return lastSync.ToOption();
}

// Explicit creation
public Option<string> GetConfigValue(string key)
{
    if (_config.TryGetValue(key, out var value))
        return Option.Some(value);
    
    return Option.None<string>();
}
```

### Chaining Options

```csharp
public Option<string> GetUserEmailDomain(int userId)
{
    return FindUser(userId)                    // Option<User>
        .Map(user => user.Email)                // Option<string>
        .Map(email => email.Split('@'))         // Option<string[]>
        .Bind(parts => parts.Length > 1 
            ? Option.Some(parts[1]) 
            : Option.None<string>())             // Option<string>
        .Map(domain => domain.ToLower());       // Option<string>
}
```

### Option with Result

```csharp
public async Task<Result<User, string>> GetRequiredUserAsync(int userId, CancellationToken ct)
{
    var userOption = await FindUserAsync(userId, ct);
    
    return userOption.ToResult(() => $"User {userId} not found");
}

public Result<decimal, string> CalculateShipping(int userId)
{
    return GetUserAddress(userId)              // Option<Address>
        .ToResult(() => "No shipping address on file")
        .Bind(address => GetShippingRate(address.ZipCode));
}
```

---

## Unit Type Examples

### Validation Without Return Value

```csharp
public Result<Unit, string> ValidateUserInput(UserInput input)
{
    if (string.IsNullOrWhiteSpace(input.Name))
        return "Name is required";
    
    if (input.Age < 0 || input.Age > 150)
        return "Age must be between 0 and 150";
    
    if (!input.Email.Contains("@"))
        return "Invalid email format";
    
    return Unit.Value;  // Success, no value needed
}
```

### Side-Effect Operations

```csharp
public async Task<Result<Unit, Exception>> SaveSettingsAsync(Settings settings, CancellationToken ct)
{
    return await Try.RunAsync(async () =>
    {
        await _repository.SaveAsync(settings, ct);
        _logger.LogInformation("Settings saved successfully");
    }).Map(_ => Unit.Value);
}

public Result<Unit, string> DeleteFile(string path)
{
    return Try.Run(() => File.Delete(path))
        .Map(_ => Unit.Value)
        .MapFailure(ex => $"Failed to delete file: {ex.Message}");
}
```

---

## Try Wrapper Examples

### Wrapping File I/O

```csharp
public Result<string, Exception> ReadConfigFile(string path)
{
    return Try.Run(() => File.ReadAllText(path));
}

public async Task<Result<byte[], Exception>> ReadFileBytesAsync(string path, CancellationToken ct)
{
    return await Try.RunAsync(async () => 
        await File.ReadAllBytesAsync(path, ct));
}
```

### Wrapping HTTP Calls

```csharp
public async Task<Result<UserData, Exception>> FetchUserDataAsync(string userId, CancellationToken ct)
{
    return await Try.RunAsync(async () =>
    {
        var response = await _httpClient.GetAsync($"api/users/{userId}", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserData>(ct);
    });
}
```

### Wrapping JSON Serialization

```csharp
public Result<string, Exception> SerializeToJson<T>(T obj)
{
    return Try.Run(() => JsonSerializer.Serialize(obj));
}

public Result<T, Exception> DeserializeFromJson<T>(string json)
{
    return Try.Run(() => JsonSerializer.Deserialize<T>(json));
}
```

---

## Testing Patterns

### Testing Result Success Cases

```csharp
[Fact]
public void GetUser_ExistingId_ReturnsSuccessWithCorrectUser()
{
    var result = _service.GetUser(42);
    
    result.Match(
        onSuccess: user =>
        {
            user.Id.ShouldBe(42);
            user.Name.ShouldNotBeNullOrWhiteSpace();
        },
        onFailure: error => throw new Exception($"Expected success but got error: {error}")
    );
}

[Fact]
public async Task SyncFile_ValidPath_ReturnsSuccess()
{
    var result = await _service.SyncFileAsync("local.txt", "remote.txt", TestContext.Current.CancellationToken);
    
    result.Match(
        onSuccess: syncResult => syncResult.BytesSynced.ShouldBeGreaterThan(0),
        onFailure: _ => throw new Exception("Expected successful sync")
    );
}
```

### Testing Result Failure Cases

```csharp
[Fact]
public void GetUser_InvalidId_ReturnsError()
{
    var result = _service.GetUser(-1);
    
    result.Match(
        onSuccess: _ => throw new Exception("Expected error but got success"),
        onFailure: error => error.ShouldContain("Invalid")
    );
}

[Fact]
public async Task SyncFile_NonExistentPath_ReturnsError()
{
    var result = await _service.SyncFileAsync("nonexistent.txt", "remote.txt", TestContext.Current.CancellationToken);
    
    result.Match(
        onSuccess: _ => throw new Exception("Expected error for nonexistent file"),
        onFailure: error => error.ShouldContain("not found")
    );
}
```

### Testing Option Cases

```csharp
[Fact]
public void FindUser_ExistingEmail_ReturnsSome()
{
    var option = _service.FindUserByEmail("test@example.com");
    
    option.IsSome().ShouldBeTrue();
    option.Match(
        onSome: user => user.Email.ShouldBe("test@example.com"),
        onNone: () => throw new Exception("Expected Some")
    );
}

[Fact]
public void FindUser_NonExistentEmail_ReturnsNone()
{
    var option = _service.FindUserByEmail("nonexistent@example.com");
    
    option.IsNone().ShouldBeTrue();
}

[Fact]
public void FindUser_NullEmail_ReturnsNone()
{
    var option = _service.FindUserByEmail(null);
    
    option.IsNone().ShouldBeTrue();
}
```

---

## Real-World Composition Example

```csharp
public class OrderProcessingService
{
    public async Task<Result<OrderConfirmation, string>> ProcessOrderAsync(
        int userId, 
        List<int> productIds,
        CancellationToken ct)
    {
        return await ValidateUser(userId)
            .ToResult(() => "Invalid user")
            .BindAsync(async user => await ValidateProductAvailability(productIds, ct))
            .BindAsync(async products => await CalculateOrderTotal(products, ct))
            .BindAsync(async total => await ValidatePaymentMethod(user.PaymentMethodId, total, ct))
            .BindAsync(async payment => await ChargePayment(payment, ct))
            .BindAsync(async charge => await CreateOrder(userId, productIds, charge, ct))
            .BindAsync(async order => await SendConfirmationEmail(order, ct))
            .MapFailure(ex => $"Order processing failed: {ex.Message}");
    }
    
    private Option<User> ValidateUser(int userId) => _userRepository.FindUser(userId);
    
    private async Task<Result<List<Product>, Exception>> ValidateProductAvailability(
        List<int> productIds, 
        CancellationToken ct)
    {
        return await Try.RunAsync(async () =>
        {
            var products = new List<Product>();
            foreach (var id in productIds)
            {
                var product = await _productRepository.GetByIdAsync(id, ct);
                if (product == null || !product.InStock)
                    throw new InvalidOperationException($"Product {id} not available");
                products.Add(product);
            }
            return products;
        });
    }
    
    private async Task<Result<decimal, Exception>> CalculateOrderTotal(
        List<Product> products, 
        CancellationToken ct)
    {
        return await Try.RunAsync(async () =>
        {
            var subtotal = products.Sum(p => p.Price);
            var tax = await _taxService.CalculateTaxAsync(subtotal, ct);
            return subtotal + tax;
        });
    }
    
    // ... other methods
}
```

---

## See Also

- **Quick Reference**: [`/docs/copilot/quick-reference.md`](/docs/copilot/quick-reference.md)
- **Common Scenarios**: [`/docs/copilot/common-scenarios.md`](/docs/copilot/common-scenarios.md)
- **Blog Series**: [`/docs/blogs/`](/docs/blogs/)
