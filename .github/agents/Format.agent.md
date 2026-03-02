---
description: Format code according to specified guidelines.
---

# Format Agent

## Mission: Format code according to specified guidelines.

## Guidelines
- apply consistent formatting based on the solution's .editorconfig settings
- ensure code follows the style conventions below

## Incorrect Record (or class) Formatting
```csharp
public sealed record FolderNodeState(
    string Id,
    string? ParentId,
    string Name,
    bool IsSelected,
    bool IsExpanded,
    int SortOrder);
````

## Correct Record (or class) Formatting
```csharp
public sealed record FolderNodeState(string Id, string? ParentId, string Name, bool IsSelected, bool IsExpanded, int SortOrder);
```

## Incorrect Method Formatting
```csharp
public static bool ReplaceNodeInCollection(
  ObservableCollection<FolderNode> collection,
  FolderNode target,
  FolderNode replacement)
{
  // Implementation omitted for brevity
}
```

## Correct Method Formatting
```csharp
public static bool ReplaceNodeInCollection(ObservableCollection<FolderNode> collection, FolderNode target, FolderNode replacement)
{
  // Implementation omitted for brevity
}
```
