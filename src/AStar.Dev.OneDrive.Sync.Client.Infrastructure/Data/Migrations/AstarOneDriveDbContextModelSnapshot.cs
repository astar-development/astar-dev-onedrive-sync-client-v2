using System;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Migrations;

[DbContext(typeof(ClientDbContext))]
partial class ClientDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        _ = modelBuilder
            .HasAnnotation("ProductVersion", "10.0.1");

        _ = modelBuilder.Entity("AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities.AccountEntity", b =>
            {
                _ = b.Property<string>("Id")
                    .HasMaxLength(64)
                    .HasColumnType("TEXT");

                _ = b.Property<DateTime>("CreatedUtc")
                    .HasColumnType("TEXT");

                _ = b.Property<string>("DisplayName")
                    .HasMaxLength(200)
                    .HasColumnType("TEXT");

                _ = b.Property<string>("Email")
                    .IsRequired()
                    .HasMaxLength(320)
                    .HasColumnType("TEXT");

                _ = b.Property<bool>("IsActive")
                    .HasColumnType("INTEGER");

                _ = b.Property<long>("QuotaBytes")
                    .HasColumnType("INTEGER");

                _ = b.Property<DateTime>("UpdatedUtc")
                    .HasColumnType("TEXT");

                _ = b.Property<long>("UsedBytes")
                    .HasColumnType("INTEGER");

                _ = b.HasKey("Id");

                _ = b.HasIndex("Email")
                    .IsUnique()
                    .HasDatabaseName("UX_Accounts_Email");

                _ = b.HasIndex("IsActive")
                    .HasDatabaseName("IX_Accounts_IsActive");

                _ = b.ToTable("Accounts");
            });

        _ = modelBuilder.Entity("AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities.SettingEntity", b =>
            {
                _ = b.Property<string>("Id")
                    .HasMaxLength(36)
                    .HasColumnType("TEXT");

                _ = b.Property<string>("Key")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("TEXT");

                _ = b.Property<DateTime>("UpdatedUtc")
                    .HasColumnType("TEXT");

                _ = b.Property<string>("Value")
                    .IsRequired()
                    .HasMaxLength(1000)
                    .HasColumnType("TEXT");

                _ = b.HasKey("Id");

                _ = b.HasIndex("Key")
                    .IsUnique()
                    .HasDatabaseName("UX_Settings_Key");

                _ = b.ToTable("Settings");
            });

        _ = modelBuilder.Entity("AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities.SyncFileEntity", b =>
            {
                _ = b.Property<string>("Id")
                    .HasMaxLength(36)
                    .HasColumnType("TEXT");

                _ = b.Property<string>("AccountId")
                    .IsRequired()
                    .HasMaxLength(64)
                    .HasColumnType("TEXT");

                _ = b.Property<string>("CTag")
                    .HasMaxLength(200)
                    .HasColumnType("TEXT");

                _ = b.Property<DateTime>("CreatedUtc")
                    .HasColumnType("TEXT");

                _ = b.Property<string>("ETag")
                    .HasMaxLength(200)
                    .HasColumnType("TEXT");

                _ = b.Property<bool>("IsExpanded")
                    .HasColumnType("INTEGER");

                _ = b.Property<bool>("IsSelected")
                    .HasColumnType("INTEGER");

                _ = b.Property<DateTime?>("LastSyncUtc")
                    .HasColumnType("TEXT");

                _ = b.Property<string>("LocalPath")
                    .IsRequired()
                    .HasMaxLength(1024)
                    .HasColumnType("TEXT");

                _ = b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(260)
                    .HasColumnType("TEXT");

                _ = b.Property<string>("ItemType")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("TEXT");

                _ = b.Property<string>("ParentId")
                    .HasMaxLength(36)
                    .HasColumnType("TEXT");

                _ = b.Property<string>("RemotePath")
                    .IsRequired()
                    .HasMaxLength(1024)
                    .HasColumnType("TEXT");

                _ = b.Property<long?>("SizeBytes")
                    .HasColumnType("INTEGER");

                _ = b.Property<int>("SortOrder")
                    .HasColumnType("INTEGER");

                _ = b.Property<DateTime>("UpdatedUtc")
                    .HasColumnType("TEXT");

                _ = b.HasKey("Id");

                _ = b.HasIndex("AccountId")
                    .HasDatabaseName("IX_SyncFiles_AccountId");

                _ = b.HasIndex("AccountId", "LocalPath")
                    .IsUnique()
                    .HasDatabaseName("UX_SyncFiles_Account_LocalPath");

                _ = b.HasIndex("AccountId", "RemotePath")
                    .IsUnique()
                    .HasDatabaseName("UX_SyncFiles_Account_RemotePath");

                _ = b.HasIndex("ParentId")
                    .HasDatabaseName("IX_SyncFiles_ParentId");

                _ = b.ToTable("SyncFiles", t =>
                    {
                        _ = t.HasCheckConstraint("CK_SyncFiles_LocalPath_Length", "length(LocalPath) <= 1024");
                        _ = t.HasCheckConstraint("CK_SyncFiles_Name_Length", "length(Name) <= 260");
                        _ = t.HasCheckConstraint("CK_SyncFiles_RemotePath_Length", "length(RemotePath) <= 1024");
                    });
            });

        _ = modelBuilder.Entity("AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities.SyncFileEntity", b =>
            {
                _ = b.HasOne("AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities.AccountEntity", "Account")
                    .WithMany("SyncFiles")
                    .HasForeignKey("AccountId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired()
                    .HasConstraintName("FK_SyncFiles_Accounts_AccountId");

                _ = b.HasOne("AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities.SyncFileEntity", "Parent")
                    .WithMany("Children")
                    .HasForeignKey("ParentId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_SyncFiles_SyncFiles_ParentId");

                _ = b.Navigation("Account");

                _ = b.Navigation("Parent");
            });

        _ = modelBuilder.Entity("AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities.AccountEntity", b => _ = b.Navigation("SyncFiles"));

        _ = modelBuilder.Entity("AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities.SyncFileEntity", b => _ = b.Navigation("Children"));
    }
}
