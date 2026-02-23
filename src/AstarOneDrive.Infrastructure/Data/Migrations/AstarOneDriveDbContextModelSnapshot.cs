using System;
using AstarOneDrive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AstarOneDrive.Infrastructure.Data.Migrations;

[DbContext(typeof(AstarOneDriveDbContext))]
partial class AstarOneDriveDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.1");

        modelBuilder.Entity("AstarOneDrive.Infrastructure.Data.Entities.AccountEntity", b =>
            {
                b.Property<string>("Id")
                    .HasMaxLength(64)
                    .HasColumnType("TEXT");

                b.Property<DateTime>("CreatedUtc")
                    .HasColumnType("TEXT");

                b.Property<string>("DisplayName")
                    .HasMaxLength(200)
                    .HasColumnType("TEXT");

                b.Property<string>("Email")
                    .IsRequired()
                    .HasMaxLength(320)
                    .HasColumnType("TEXT");

                b.Property<bool>("IsActive")
                    .HasColumnType("INTEGER");

                b.Property<long>("QuotaBytes")
                    .HasColumnType("INTEGER");

                b.Property<DateTime>("UpdatedUtc")
                    .HasColumnType("TEXT");

                b.Property<long>("UsedBytes")
                    .HasColumnType("INTEGER");

                b.HasKey("Id");

                b.HasIndex("Email")
                    .IsUnique()
                    .HasDatabaseName("UX_Accounts_Email");

                b.HasIndex("IsActive")
                    .HasDatabaseName("IX_Accounts_IsActive");

                b.ToTable("Accounts");
            });

        modelBuilder.Entity("AstarOneDrive.Infrastructure.Data.Entities.SettingEntity", b =>
            {
                b.Property<string>("Id")
                    .HasMaxLength(36)
                    .HasColumnType("TEXT");

                b.Property<string>("Key")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("TEXT");

                b.Property<DateTime>("UpdatedUtc")
                    .HasColumnType("TEXT");

                b.Property<string>("Value")
                    .IsRequired()
                    .HasMaxLength(1000)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("Key")
                    .IsUnique()
                    .HasDatabaseName("UX_Settings_Key");

                b.ToTable("Settings");
            });

        modelBuilder.Entity("AstarOneDrive.Infrastructure.Data.Entities.SyncFileEntity", b =>
            {
                b.Property<string>("Id")
                    .HasMaxLength(36)
                    .HasColumnType("TEXT");

                b.Property<string>("AccountId")
                    .IsRequired()
                    .HasMaxLength(64)
                    .HasColumnType("TEXT");

                b.Property<string>("CTag")
                    .HasMaxLength(200)
                    .HasColumnType("TEXT");

                b.Property<DateTime>("CreatedUtc")
                    .HasColumnType("TEXT");

                b.Property<string>("ETag")
                    .HasMaxLength(200)
                    .HasColumnType("TEXT");

                b.Property<bool>("IsExpanded")
                    .HasColumnType("INTEGER");

                b.Property<bool>("IsSelected")
                    .HasColumnType("INTEGER");

                b.Property<DateTime?>("LastSyncUtc")
                    .HasColumnType("TEXT");

                b.Property<string>("LocalPath")
                    .IsRequired()
                    .HasMaxLength(1024)
                    .HasColumnType("TEXT");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(260)
                    .HasColumnType("TEXT");

                b.Property<string>("ItemType")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("TEXT");

                b.Property<string>("ParentId")
                    .HasMaxLength(36)
                    .HasColumnType("TEXT");

                b.Property<string>("RemotePath")
                    .IsRequired()
                    .HasMaxLength(1024)
                    .HasColumnType("TEXT");

                b.Property<long?>("SizeBytes")
                    .HasColumnType("INTEGER");

                b.Property<int>("SortOrder")
                    .HasColumnType("INTEGER");

                b.Property<DateTime>("UpdatedUtc")
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("AccountId")
                    .HasDatabaseName("IX_SyncFiles_AccountId");

                b.HasIndex("AccountId", "LocalPath")
                    .IsUnique()
                    .HasDatabaseName("UX_SyncFiles_Account_LocalPath");

                b.HasIndex("AccountId", "RemotePath")
                    .IsUnique()
                    .HasDatabaseName("UX_SyncFiles_Account_RemotePath");

                b.HasIndex("ParentId")
                    .HasDatabaseName("IX_SyncFiles_ParentId");

                b.ToTable("SyncFiles", t =>
                    {
                        t.HasCheckConstraint("CK_SyncFiles_LocalPath_Length", "length(LocalPath) <= 1024");
                        t.HasCheckConstraint("CK_SyncFiles_Name_Length", "length(Name) <= 260");
                        t.HasCheckConstraint("CK_SyncFiles_RemotePath_Length", "length(RemotePath) <= 1024");
                    });
            });

        modelBuilder.Entity("AstarOneDrive.Infrastructure.Data.Entities.SyncFileEntity", b =>
            {
                b.HasOne("AstarOneDrive.Infrastructure.Data.Entities.AccountEntity", "Account")
                    .WithMany("SyncFiles")
                    .HasForeignKey("AccountId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired()
                    .HasConstraintName("FK_SyncFiles_Accounts_AccountId");

                b.HasOne("AstarOneDrive.Infrastructure.Data.Entities.SyncFileEntity", "Parent")
                    .WithMany("Children")
                    .HasForeignKey("ParentId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_SyncFiles_SyncFiles_ParentId");

                b.Navigation("Account");

                b.Navigation("Parent");
            });

        modelBuilder.Entity("AstarOneDrive.Infrastructure.Data.Entities.AccountEntity", b =>
            {
                b.Navigation("SyncFiles");
            });

        modelBuilder.Entity("AstarOneDrive.Infrastructure.Data.Entities.SyncFileEntity", b =>
            {
                b.Navigation("Children");
            });
    }
}
