using System;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Migrations;

[DbContext(typeof(AstarOneDriveDbContextModel))]
[Migration("20260223150000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.CreateTable(
            name: "Accounts",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                QuotaBytes = table.Column<long>(type: "INTEGER", nullable: false),
                UsedBytes = table.Column<long>(type: "INTEGER", nullable: false),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table => _ = table.PrimaryKey("PK_Accounts", x => x.Id));

        _ = migrationBuilder.CreateTable(
            name: "Settings",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Value = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table => _ = table.PrimaryKey("PK_Settings", x => x.Id));

        _ = migrationBuilder.CreateTable(
            name: "SyncFiles",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                AccountId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                ParentId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                Name = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                LocalPath = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                RemotePath = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                ItemType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                IsSelected = table.Column<bool>(type: "INTEGER", nullable: false),
                IsExpanded = table.Column<bool>(type: "INTEGER", nullable: false),
                LastSyncUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                CTag = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                ETag = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                SizeBytes = table.Column<long>(type: "INTEGER", nullable: true),
                SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_SyncFiles", x => x.Id);
                _ = table.CheckConstraint("CK_SyncFiles_LocalPath_Length", "length(LocalPath) <= 1024");
                _ = table.CheckConstraint("CK_SyncFiles_Name_Length", "length(Name) <= 260");
                _ = table.CheckConstraint("CK_SyncFiles_RemotePath_Length", "length(RemotePath) <= 1024");
                _ = table.ForeignKey(
                    name: "FK_SyncFiles_Accounts_AccountId",
                    column: x => x.AccountId,
                    principalTable: "Accounts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                _ = table.ForeignKey(
                    name: "FK_SyncFiles_SyncFiles_ParentId",
                    column: x => x.ParentId,
                    principalTable: "SyncFiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateIndex(
            name: "IX_Accounts_IsActive",
            table: "Accounts",
            column: "IsActive");

        _ = migrationBuilder.CreateIndex(
            name: "UX_Accounts_Email",
            table: "Accounts",
            column: "Email",
            unique: true);

        _ = migrationBuilder.CreateIndex(
            name: "UX_Settings_Key",
            table: "Settings",
            column: "Key",
            unique: true);

        _ = migrationBuilder.CreateIndex(
            name: "IX_SyncFiles_AccountId",
            table: "SyncFiles",
            column: "AccountId");

        _ = migrationBuilder.CreateIndex(
            name: "UX_SyncFiles_Account_LocalPath",
            table: "SyncFiles",
            columns: new[] { "AccountId", "LocalPath" },
            unique: true);

        _ = migrationBuilder.CreateIndex(
            name: "UX_SyncFiles_Account_RemotePath",
            table: "SyncFiles",
            columns: new[] { "AccountId", "RemotePath" },
            unique: true);

        _ = migrationBuilder.CreateIndex(
            name: "IX_SyncFiles_ParentId",
            table: "SyncFiles",
            column: "ParentId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(
            name: "Settings");

        _ = migrationBuilder.DropTable(
            name: "SyncFiles");

        _ = migrationBuilder.DropTable(
            name: "Accounts");
    }
}
