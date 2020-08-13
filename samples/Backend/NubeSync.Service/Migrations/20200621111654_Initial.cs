using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NubeSync.Service.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Operations",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ClusteredIndex = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    InstallationId = table.Column<string>(nullable: true),
                    ItemId = table.Column<string>(nullable: false),
                    OldValue = table.Column<string>(nullable: true),
                    ProcessingType = table.Column<byte>(nullable: false),
                    Property = table.Column<string>(nullable: true),
                    ServerUpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    TableName = table.Column<string>(nullable: false),
                    Type = table.Column<byte>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "TodoItems",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ClusteredIndex = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(nullable: true),
                    ServerUpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    IsChecked = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoItems", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Operations_ClusteredIndex",
                table: "Operations",
                column: "ClusteredIndex")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Operations_ItemId_Property_CreatedAt",
                table: "Operations",
                columns: new[] { "ItemId", "Property", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Operations_ItemId_TableName_ServerUpdatedAt_ProcessingType_InstallationId",
                table: "Operations",
                columns: new[] { "ItemId", "TableName", "ServerUpdatedAt", "ProcessingType", "InstallationId" });

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_ClusteredIndex",
                table: "TodoItems",
                column: "ClusteredIndex")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_UserId_ServerUpdatedAt",
                table: "TodoItems",
                columns: new[] { "UserId", "ServerUpdatedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Operations");

            migrationBuilder.DropTable(
                name: "TodoItems");
        }
    }
}
