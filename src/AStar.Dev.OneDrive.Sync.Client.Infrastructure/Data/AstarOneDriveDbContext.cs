using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the OneDrive sync client SQLite database.
/// </summary>
public sealed class AstarOneDriveDbContextModel(DbContextOptions<AstarOneDriveDbContextModel> options) : DbContext(options)
{
    /// <summary>
    /// Gets the database set for application settings.
    /// </summary>
    public DbSet<SettingEntity> Settings => Set<SettingEntity>();

    /// <summary>
    /// Gets the database set for OneDrive accounts.
    /// </summary>
    public DbSet<AccountEntity> Accounts => Set<AccountEntity>();

    /// <summary>
    /// Gets the database set for synchronized files.
    /// </summary>
    public DbSet<SyncFileEntity> SyncFiles => Set<SyncFileEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(AstarOneDriveDbContextModel).Assembly);
}
