namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents a remote delta page response with pagination/checkpoint tokens.
/// </summary>
/// <param name="Changes">The change entries in the current page.</param>
/// <param name="NextPageToken">The next-page token, if pagination should continue.</param>
/// <param name="DeltaToken">The final checkpoint token, when page iteration is complete.</param>
public sealed record RemoteDeltaPage(
    IReadOnlyList<RemoteDeltaItem> Changes,
    string? NextPageToken,
    string? DeltaToken);