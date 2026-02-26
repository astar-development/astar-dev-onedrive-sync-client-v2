namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;

public sealed record AccountState(
    string Id,
    string Email,
    long QuotaBytes,
    long UsedBytes);
