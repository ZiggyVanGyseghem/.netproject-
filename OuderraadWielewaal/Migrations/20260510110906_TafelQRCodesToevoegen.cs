using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OuderraadWielewaal.Migrations
{
    /// <inheritdoc />
    public partial class TafelQRCodesToevoegen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Actief",
                table: "Tafels",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UniekeCode",
                table: "Tafels",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Tafels",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Actief", "UniekeCode" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Tafels",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Actief", "UniekeCode" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Tafels",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Actief", "UniekeCode" },
                values: new object[] { false, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Actief",
                table: "Tafels");

            migrationBuilder.DropColumn(
                name: "UniekeCode",
                table: "Tafels");
        }
    }
}
