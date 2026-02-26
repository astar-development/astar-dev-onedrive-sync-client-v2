using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Configurations;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;

public sealed class AstarOneDriveDbContextModel(DbContextOptions<AstarOneDriveDbContextModel> options) : DbContext(options)
{
    public DbSet<SettingEntity> Settings => Set<SettingEntity>();

    public DbSet<AccountEntity> Accounts => Set<AccountEntity>();

    public DbSet<SyncFileEntity> SyncFiles => Set<SyncFileEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(AstarOneDriveDbContextModel).Assembly);
}
