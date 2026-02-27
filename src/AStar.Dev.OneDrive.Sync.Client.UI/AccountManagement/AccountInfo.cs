namespace AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;

/// <summary>
/// Represents information about a OneDrive account, including storage usage details.
/// </summary>
/// <param name="Id">The unique identifier of the account.</param>
/// <param name="Email">The email address associated with the account.</param>
/// <param name="QuotaBytes">The total storage quota of the account in bytes.</param>
/// <param name="UsedBytes">The amount of storage used by the account in bytes.</param>
public record AccountInfo(string Id, string Email, long QuotaBytes, long UsedBytes)
{
    public string StorageUsed => $"{UsedBytes} / {QuotaBytes} bytes";
}
