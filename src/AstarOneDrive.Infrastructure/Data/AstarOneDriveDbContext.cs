using AstarOneDrive.Infrastructure.Data.Configurations;
using AstarOneDrive.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AstarOneDrive.Infrastructure.Data;

public sealed class AstarOneDriveDbContext(DbContextOptions<AstarOneDriveDbContext> options) : DbContext(options)
{
    public DbSet<SettingEntity> Settings => Set<SettingEntity>();

    public DbSet<AccountEntity> Accounts => Set<AccountEntity>();

    public DbSet<SyncFileEntity> SyncFiles => Set<SyncFileEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SettingConfiguration());
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new SyncFileConfiguration());
    }
}
