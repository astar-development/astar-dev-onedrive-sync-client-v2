namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;

public sealed class AccountEntity
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public long QuotaBytes { get; set; }

    public long UsedBytes { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }

    public ICollection<SyncFileEntity> SyncFiles { get; set; } = [];
}
