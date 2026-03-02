namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;

/// <summary>
/// Database entity representing a OneDrive account.
/// </summary>
public sealed class AccountEntity
{
    /// <summary>
    /// Gets or sets the unique identifier of the account.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address associated with the account.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the account.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the total storage quota in bytes.
    /// </summary>
    public long QuotaBytes { get; set; }

    /// <summary>
    /// Gets or sets the amount of storage used in bytes.
    /// </summary>
    public long UsedBytes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the account is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the account was created.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the account was last updated.
    /// </summary>
    public DateTime UpdatedUtc { get; set; }

    /// <summary>
    /// Gets or sets the collection of files synchronized for this account.
    /// </summary>
    public ICollection<SyncFileEntity> SyncFiles { get; set; } = [];
}
