using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Migrations;

[DbContext(typeof(AstarOneDriveDbContextModel))]
[Migration("20260310032000_AddAccountLocalSyncRootPath")]
public partial class AddAccountLocalSyncRootPath : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
        => _ = migrationBuilder.AddColumn<string>(
            name: "LocalSyncRootPath",
            table: "Accounts",
            type: "TEXT",
            maxLength: 1024,
            nullable: false,
            defaultValue: string.Empty);

    protected override void Down(MigrationBuilder migrationBuilder)
        => _ = migrationBuilder.DropColumn(
            name: "LocalSyncRootPath",
            table: "Accounts");
}
