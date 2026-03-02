namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;

/// <summary>
/// Database entity representing an application setting as a key-value pair.
/// </summary>
public sealed class SettingEntity
{
    /// <summary>
    /// Gets or sets the unique identifier of the setting.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the setting key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the setting value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC timestamp when the setting was last updated.
    /// </summary>
    public DateTime UpdatedUtc { get; set; }
}
