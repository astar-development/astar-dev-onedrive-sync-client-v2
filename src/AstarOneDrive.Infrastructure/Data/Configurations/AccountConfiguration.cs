using AstarOneDrive.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AstarOneDrive.Infrastructure.Data.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<AccountEntity>
{
    public void Configure(EntityTypeBuilder<AccountEntity> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(x => x.DisplayName)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(x => x.QuotaBytes)
            .IsRequired();

        builder.Property(x => x.UsedBytes)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedUtc)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasDatabaseName("UX_Accounts_Email");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_Accounts_IsActive");
    }
}
