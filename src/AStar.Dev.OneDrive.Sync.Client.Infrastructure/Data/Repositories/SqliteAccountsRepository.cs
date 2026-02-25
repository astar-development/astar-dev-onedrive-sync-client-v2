using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;

public sealed class SqliteAccountsRepository(string? databasePath = null)
{
    public async Task SaveAsync(IReadOnlyList<AccountState> accounts, CancellationToken cancellationToken = default)
    {
        await using AStar.Dev.OneDrive.Sync.ClientDbContext context = AStar.Dev.OneDrive.Sync.ClientDbContextFactory.Create(databasePath);
        List<AccountEntity> existing = await context.Accounts.ToListAsync(cancellationToken);
        context.Accounts.RemoveRange(existing);

        DateTime now = DateTime.UtcNow;
        foreach(AccountState account in accounts)
        {
            _ = context.Accounts.Add(new AccountEntity
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

        _ = await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AccountState>> LoadAsync(CancellationToken cancellationToken = default)
    {
        await using AStar.Dev.OneDrive.Sync.ClientDbContext context = AStar.Dev.OneDrive.Sync.ClientDbContextFactory.Create(databasePath);
        List<AccountState> accounts = await context.Accounts
            .AsNoTracking()
            .OrderBy(x => x.Email)
            .Select(x => new AccountState(x.Id, x.Email, x.QuotaBytes, x.UsedBytes))
            .ToListAsync(cancellationToken);

        return accounts;
    }
}
