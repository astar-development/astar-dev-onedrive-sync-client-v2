namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents current account profile and session metadata state.
/// </summary>
/// <param name="Profile">The OneDrive account profile.</param>
/// <param name="Session">The account session metadata.</param>
public sealed record AccountSessionState(OneDriveAccountProfile Profile, AccountSessionMetadata Session);