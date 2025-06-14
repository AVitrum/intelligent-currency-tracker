using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class updated_report_and_post : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileLinks_Posts_PostId",
                table: "FileLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_FileLinks_Reports_ReportId",
                table: "FileLinks");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReportId",
                table: "FileLinks",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "PostId",
                table: "FileLinks",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_FileLinks_Posts_PostId",
                table: "FileLinks",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FileLinks_Reports_ReportId",
                table: "FileLinks",
                column: "ReportId",
                principalTable: "Reports",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileLinks_Posts_PostId",
                table: "FileLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_FileLinks_Reports_ReportId",
                table: "FileLinks");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReportId",
                table: "FileLinks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PostId",
                table: "FileLinks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FileLinks_Posts_PostId",
                table: "FileLinks",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FileLinks_Reports_ReportId",
                table: "FileLinks",
                column: "ReportId",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
