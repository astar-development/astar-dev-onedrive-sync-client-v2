using AstarOneDrive.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AstarOneDrive.Infrastructure.Data.Configurations;

public sealed class SyncFileConfiguration : IEntityTypeConfiguration<SyncFileEntity>
{
    public void Configure(EntityTypeBuilder<SyncFileEntity> builder)
    {
        builder.ToTable("SyncFiles", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_SyncFiles_Name_Length", "length(Name) <= 260");
            tableBuilder.HasCheckConstraint("CK_SyncFiles_LocalPath_Length", "length(LocalPath) <= 1024");
            tableBuilder.HasCheckConstraint("CK_SyncFiles_RemotePath_Length", "length(RemotePath) <= 1024");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(x => x.AccountId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.ParentId)
            .HasMaxLength(36)
            .IsRequired(false);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(x => x.LocalPath)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(x => x.RemotePath)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(x => x.ItemType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.IsSelected)
            .IsRequired();

        builder.Property(x => x.IsExpanded)
            .IsRequired();

        builder.Property(x => x.LastSyncUtc)
            .IsRequired(false);

        builder.Property(x => x.CTag)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(x => x.ETag)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(x => x.SizeBytes)
            .IsRequired(false);

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.CreatedUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedUtc)
            .IsRequired();

        builder.HasIndex(x => x.AccountId)
            .HasDatabaseName("IX_SyncFiles_AccountId");

        builder.HasIndex(x => x.ParentId)
            .HasDatabaseName("IX_SyncFiles_ParentId");

        builder.HasIndex(x => new { x.AccountId, x.LocalPath })
            .IsUnique()
            .HasDatabaseName("UX_SyncFiles_Account_LocalPath");

        builder.HasIndex(x => new { x.AccountId, x.RemotePath })
            .IsUnique()
            .HasDatabaseName("UX_SyncFiles_Account_RemotePath");

        builder.HasOne(x => x.Account)
            .WithMany(x => x.SyncFiles)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_SyncFiles_Accounts_AccountId");

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_SyncFiles_SyncFiles_ParentId");

    }
}
