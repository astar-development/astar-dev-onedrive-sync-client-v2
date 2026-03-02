using AstarOneDrive.Infrastructure.Data.Contracts;
using AstarOneDrive.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AstarOneDrive.Infrastructure.Data.Repositories;

/// <summary>
/// SQLite repository for persisting and retrieving OneDrive account information.
/// </summary>
public sealed class SqliteAccountsRepository(string? databasePath = null)
{
    /// <summary>
    /// Saves a collection of accounts to the database, replacing all existing accounts.
    /// </summary>
    /// <param name="accounts">The accounts to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SaveAsync(IReadOnlyList<AccountState> accounts, CancellationToken cancellationToken = default)
    {
        await using var context = AstarOneDriveDbContextFactory.Create(databasePath);
        var existing = await context.Accounts.ToListAsync(cancellationToken);
        context.Accounts.RemoveRange(existing);

        var now = DateTime.UtcNow;
        foreach (var account in accounts)
        {
            context.Accounts.Add(new AccountEntity
            {
                Id = account.Id,
                Email = account.Email,
                QuotaBytes = account.QuotaBytes,
                UsedBytes = account.UsedBytes,
                IsActive = true,
                CreatedUtc = now,
                UpdatedUtc = now
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Loads all accounts from the database, ordered by email address.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of account states.</returns>
    public async Task<IReadOnlyList<AccountState>> LoadAsync(CancellationToken cancellationToken = default)
    {
        await using var context = AstarOneDriveDbContextFactory.Create(databasePath);
        var accounts = await context.Accounts
            .AsNoTracking()
            .OrderBy(x => x.Email)
            .Select(x => new AccountState(x.Id, x.Email, x.QuotaBytes, x.UsedBytes))
            .ToListAsync(cancellationToken);

        return accounts;
    }
}
