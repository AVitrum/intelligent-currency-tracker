using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class updated_api_request_log : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Rates",
                newName: "TimeStamp");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Currencies",
                newName: "TimeStamp");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ApiRequestLogs",
                newName: "TimeStamp");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ApiRequestLogs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ApiRequestLogs");

            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "Rates",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "Currencies",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "ApiRequestLogs",
                newName: "CreatedAt");
        }
    }
}
