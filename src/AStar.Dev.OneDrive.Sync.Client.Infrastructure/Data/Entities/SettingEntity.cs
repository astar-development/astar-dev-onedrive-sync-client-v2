namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;

public sealed class SettingEntity
{
    public string Id { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public DateTime UpdatedUtc { get; set; }
}
