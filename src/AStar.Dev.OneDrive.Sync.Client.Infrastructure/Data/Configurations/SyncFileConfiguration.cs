using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Configurations;

public sealed class SyncFileConfiguration : IEntityTypeConfiguration<SyncFileEntity>
{
    public void Configure(EntityTypeBuilder<SyncFileEntity> builder)
    {
        _ = builder.ToTable("SyncFiles", tableBuilder =>
        {
            _ = tableBuilder.HasCheckConstraint("CK_SyncFiles_Name_Length", "length(Name) <= 260");
            _ = tableBuilder.HasCheckConstraint("CK_SyncFiles_LocalPath_Length", "length(LocalPath) <= 1024");
            _ = tableBuilder.HasCheckConstraint("CK_SyncFiles_RemotePath_Length", "length(RemotePath) <= 1024");
        });

        _ = builder.HasKey(x => x.Id);

        _ = builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(36);

        _ = builder.Property(x => x.AccountId)
            .IsRequired()
            .HasMaxLength(64);

        _ = builder.Property(x => x.ParentId)
            .HasMaxLength(36)
            .IsRequired(false);

        _ = builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(260);

        _ = builder.Property(x => x.LocalPath)
            .IsRequired()
            .HasMaxLength(1024);

        _ = builder.Property(x => x.RemotePath)
            .IsRequired()
            .HasMaxLength(1024);

        _ = builder.Property(x => x.ItemType)
            .IsRequired()
            .HasMaxLength(20);

        _ = builder.Property(x => x.IsSelected)
            .IsRequired();

        _ = builder.Property(x => x.IsExpanded)
            .IsRequired();

        _ = builder.Property(x => x.LastSyncUtc)
            .IsRequired(false);

        _ = builder.Property(x => x.CTag)
            .HasMaxLength(200)
            .IsRequired(false);

        _ = builder.Property(x => x.ETag)
            .HasMaxLength(200)
            .IsRequired(false);

        _ = builder.Property(x => x.SizeBytes)
            .IsRequired(false);

        _ = builder.Property(x => x.SortOrder)
            .IsRequired();

        _ = builder.Property(x => x.CreatedUtc)
            .IsRequired();

        _ = builder.Property(x => x.UpdatedUtc)
            .IsRequired();

        _ = builder.HasIndex(x => x.AccountId)
            .HasDatabaseName("IX_SyncFiles_AccountId");

        _ = builder.HasIndex(x => x.ParentId)
            .HasDatabaseName("IX_SyncFiles_ParentId");

        _ = builder.HasIndex(x => new { x.AccountId, x.LocalPath })
            .IsUnique()
            .HasDatabaseName("UX_SyncFiles_Account_LocalPath");

        _ = builder.HasIndex(x => new { x.AccountId, x.RemotePath })
            .IsUnique()
            .HasDatabaseName("UX_SyncFiles_Account_RemotePath");

        _ = builder.HasOne(x => x.Account)
            .WithMany(x => x.SyncFiles)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_SyncFiles_Accounts_AccountId");

        _ = builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_SyncFiles_SyncFiles_ParentId");

    }
}
