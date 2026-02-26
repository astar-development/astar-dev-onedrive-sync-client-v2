using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Configurations;

public sealed class SettingConfiguration : IEntityTypeConfiguration<SettingEntity>
{
    public void Configure(EntityTypeBuilder<SettingEntity> builder)
    {
        _ = builder.ToTable("Settings");

        _ = builder.HasKey(x => x.Id);

        _ = builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(36);

        _ = builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);

        _ = builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(1000);

        _ = builder.Property(x => x.UpdatedUtc)
            .IsRequired();

        _ = builder.HasIndex(x => x.Key)
            .IsUnique()
            .HasDatabaseName("UX_Settings_Key");
    }
}
