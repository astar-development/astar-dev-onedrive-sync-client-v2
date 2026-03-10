using System.Text.Json;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Authentication;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

/// <summary>
/// Retrieves and maps OneDrive Graph delta pages for account-scoped sync.
/// </summary>
public sealed class OneDriveRemoteDeltaSource(
    IOneDriveGraphClient graphClient,
    IAccountSessionService accountSessionService,
    ISecureAccountTokenStore secureAccountTokenStore) : IRemoteDeltaSource
{
    /// <inheritdoc />
    public async Task<Result<RemoteDeltaPage, string>> GetDeltaPageAsync(string accountId, string scopeId, string? cursor, CancellationToken cancellationToken = default)
    {
        Result<AccountSessionState, string> sessionResult = await accountSessionService.GetValidSessionAsync(accountId, cancellationToken);
        if(sessionResult is Result<AccountSessionState, string>.Error sessionError)
        {
            return sessionError.Reason;
        }

        Result<Option<SecureAccountTokens>, string> tokenResult = await secureAccountTokenStore.LoadAsync(accountId, cancellationToken);
        if(tokenResult is Result<Option<SecureAccountTokens>, string>.Error tokenError)
        {
            return tokenError.Reason;
        }

        Option<SecureAccountTokens> tokenOption = ((Result<Option<SecureAccountTokens>, string>.Ok)tokenResult).Value;
        if(tokenOption is not Option<SecureAccountTokens>.Some tokenSome)
        {
            return "No secure tokens are available for this account.";
        }

        var path = ResolveDeltaPath(scopeId, cursor);
        Result<OneDriveGraphResponse, SyncError> graphResult = await graphClient.SendAsync(
            OneDriveGraphRequest.Get(path, tokenSome.Value.AccessToken),
            cancellationToken);
        if(graphResult is Result<OneDriveGraphResponse, SyncError>.Error graphError)
        {
            return graphError.Reason.Message;
        }

        OneDriveGraphResponse response = ((Result<OneDriveGraphResponse, SyncError>.Ok)graphResult).Value;
        return ParseDeltaPage(response.Content);
    }

    private static Result<RemoteDeltaPage, string> ParseDeltaPage(string content)
    {
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(content) ? "{}" : content);
            JsonElement root = document.RootElement;
            List<RemoteDeltaItem> changes = [.. ReadChanges(root)];
            var next = ReadString(root, "@odata.nextLink");
            var delta = ReadString(root, "@odata.deltaLink");
            return new RemoteDeltaPage(changes, next, delta);
        }
        catch(JsonException ex)
        {
            return $"Failed to parse OneDrive delta response: {ex.Message}";
        }
    }

    private static IEnumerable<RemoteDeltaItem> ReadChanges(JsonElement root)
    {
        if(!root.TryGetProperty("value", out JsonElement value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var changes = new List<RemoteDeltaItem>();
        foreach(JsonElement item in value.EnumerateArray())
        {
              var id = ReadString(item, "id") ?? Guid.NewGuid().ToString("N");
              var name = ReadString(item, "name") ?? id;
              RemoteDeltaChangeKind kind = item.TryGetProperty("deleted", out _) ? RemoteDeltaChangeKind.Deleted : RemoteDeltaChangeKind.Updated;
              var path = ResolveRemotePath(item, id, name);
            changes.Add(new RemoteDeltaItem(id, path, kind));
        }

        return changes;
    }

    private static string ResolveRemotePath(JsonElement item, string id, string name)
    {
        var parentPath = ReadString(item, "parentReference", "path");
        if(string.IsNullOrWhiteSpace(parentPath))
        {
            return $"/{name}";
        }

        var markerIndex = parentPath.IndexOf(':');
        var normalizedParent = string.IsNullOrWhiteSpace(markerIndex >= 0 ? parentPath[(markerIndex + 1)..] : parentPath)
            ? "/"
            : markerIndex >= 0 ? parentPath[(markerIndex + 1)..] : parentPath;

        normalizedParent = normalizedParent.TrimEnd('/');
        var path = string.Equals(normalizedParent, "/", StringComparison.Ordinal)
            ? $"/{name}"
            : $"{normalizedParent}/{name}";
        return string.IsNullOrWhiteSpace(path) ? $"/{id}" : path;
    }

    private static string ResolveDeltaPath(string scopeId, string? cursor)
        => !string.IsNullOrWhiteSpace(cursor)
            ? cursor
            : string.IsNullOrWhiteSpace(scopeId) || string.Equals(scopeId, "drive-root", StringComparison.OrdinalIgnoreCase)
                ? "/me/drive/root/delta"
                : scopeId.StartsWith("/", StringComparison.Ordinal)
                    ? $"/me/drive/root:{scopeId}:/delta"
                    : $"/me/drive/items/{Uri.EscapeDataString(scopeId)}/delta";

    private static string? ReadString(JsonElement element, string property)
        => element.TryGetProperty(property, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string? ReadString(JsonElement element, string parentProperty, string childProperty)
        => !element.TryGetProperty(parentProperty, out JsonElement parent) || parent.ValueKind != JsonValueKind.Object
            ? null
            : ReadString(parent, childProperty);
}