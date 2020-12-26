using Microsoft.EntityFrameworkCore.Migrations;

namespace NubeSync.Service.Migrations
{
    public partial class FixGetLastChangeForPropertyIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Operations_ItemId_Property_CreatedAt",
                table: "Operations");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_ItemId_TableName_Property_CreatedAt",
                table: "Operations",
                columns: new[] { "ItemId", "TableName", "Property", "CreatedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Operations_ItemId_TableName_Property_CreatedAt",
                table: "Operations");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_ItemId_Property_CreatedAt",
                table: "Operations",
                columns: new[] { "ItemId", "Property", "CreatedAt" });
        }
    }
}
