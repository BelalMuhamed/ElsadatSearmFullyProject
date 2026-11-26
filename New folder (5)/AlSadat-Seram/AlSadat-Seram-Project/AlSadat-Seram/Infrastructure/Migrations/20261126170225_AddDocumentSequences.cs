using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentSequences",
                columns: table => new
                {
                    DocumentType = table.Column<byte>(type: "tinyint", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    LastNumber = table.Column<int>(type: "int", nullable: false),
                    MinDigits = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSequences", x => x.DocumentType);
                });

            migrationBuilder.InsertData(
                table: "DocumentSequences",
                columns: new[] { "DocumentType", "LastNumber", "MinDigits", "Prefix" },
                values: new object[,]
                {
                    { (byte)1, 0, (byte)4, "INV" },
                    { (byte)2, 0, (byte)4, "PUR" },
                    { (byte)3, 0, (byte)4, "RC" },
                    { (byte)4, 0, (byte)4, "RET" },
                    { (byte)5, 0, (byte)4, "TR" },
                    { (byte)6, 0, (byte)4, "CNT" },
                    { (byte)7, 0, (byte)4, "INS" },
                    { (byte)8, 0, (byte)4, "W" },
                    { (byte)9, 0, (byte)4, "JE" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentSequences");
        }
    }
}
