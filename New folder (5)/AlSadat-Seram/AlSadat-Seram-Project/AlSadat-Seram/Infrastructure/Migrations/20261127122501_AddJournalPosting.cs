using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalPosting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "journalEntriesDetails",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Desc",
                table: "journalEntries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "journalEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "journalEntries",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntryNumber",
                table: "journalEntries",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemGenerated",
                table: "journalEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReversalOfEntryId",
                table: "journalEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceId",
                table: "journalEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "SourceType",
                table: "journalEntries",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "journalEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "journalEntries",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_journalEntries_EntryNumber",
                table: "journalEntries",
                column: "EntryNumber",
                unique: true,
                filter: "[EntryNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_journalEntries_ReversalOfEntryId",
                table: "journalEntries",
                column: "ReversalOfEntryId",
                unique: true,
                filter: "[ReversalOfEntryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_journalEntries_SourceType_SourceId",
                table: "journalEntries",
                columns: new[] { "SourceType", "SourceId" });

            migrationBuilder.AddForeignKey(
                name: "FK_journalEntries_journalEntries_ReversalOfEntryId",
                table: "journalEntries",
                column: "ReversalOfEntryId",
                principalTable: "journalEntries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_journalEntries_journalEntries_ReversalOfEntryId",
                table: "journalEntries");

            migrationBuilder.DropIndex(
                name: "IX_journalEntries_EntryNumber",
                table: "journalEntries");

            migrationBuilder.DropIndex(
                name: "IX_journalEntries_ReversalOfEntryId",
                table: "journalEntries");

            migrationBuilder.DropIndex(
                name: "IX_journalEntries_SourceType_SourceId",
                table: "journalEntries");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "journalEntriesDetails");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "journalEntries");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "journalEntries");

            migrationBuilder.DropColumn(
                name: "EntryNumber",
                table: "journalEntries");

            migrationBuilder.DropColumn(
                name: "IsSystemGenerated",
                table: "journalEntries");

            migrationBuilder.DropColumn(
                name: "ReversalOfEntryId",
                table: "journalEntries");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "journalEntries");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "journalEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "journalEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "journalEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Desc",
                table: "journalEntries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
