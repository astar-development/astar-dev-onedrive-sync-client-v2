namespace AstarOneDrive.Infrastructure.Data.Contracts;

public sealed record AccountState(
    string Id,
    string Email,
    long QuotaBytes,
    long UsedBytes);
