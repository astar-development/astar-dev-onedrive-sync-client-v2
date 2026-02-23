using AstarOneDrive.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AstarOneDrive.Infrastructure.Data.Configurations;

public sealed class SettingConfiguration : IEntityTypeConfiguration<SettingEntity>
{
    public void Configure(EntityTypeBuilder<SettingEntity> builder)
    {
        builder.ToTable("Settings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.UpdatedUtc)
            .IsRequired();

        builder.HasIndex(x => x.Key)
            .IsUnique()
            .HasDatabaseName("UX_Settings_Key");
    }
}
