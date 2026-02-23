namespace AstarOneDrive.UI.AccountManagement;

public record AccountInfo(string Id, string Email, long QuotaBytes, long UsedBytes)
{
    public string StorageUsed => $"{UsedBytes} / {QuotaBytes} bytes";
}