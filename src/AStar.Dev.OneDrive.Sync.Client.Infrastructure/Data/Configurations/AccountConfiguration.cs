using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<AccountEntity>
{
    public void Configure(EntityTypeBuilder<AccountEntity> builder)
    {
        _ = builder.ToTable("Accounts");

        _ = builder.HasKey(x => x.Id);

        _ = builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(64);

        _ = builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(320);

        _ = builder.Property(x => x.DisplayName)
            .HasMaxLength(200)
            .IsRequired(false);

        _ = builder.Property(x => x.QuotaBytes)
            .IsRequired();

        _ = builder.Property(x => x.UsedBytes)
            .IsRequired();

        _ = builder.Property(x => x.IsActive)
            .IsRequired();

        _ = builder.Property(x => x.CreatedUtc)
            .IsRequired();

        _ = builder.Property(x => x.UpdatedUtc)
            .IsRequired();

        _ = builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasDatabaseName("UX_Accounts_Email");

        _ = builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_Accounts_IsActive");
    }
}
