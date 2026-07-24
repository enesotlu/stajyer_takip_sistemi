using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajyerTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DevamKonumDogrulama : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Boylam",
                table: "DevamKayitlari",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Enlem",
                table: "DevamKayitlari",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Boylam",
                table: "DevamKayitlari");

            migrationBuilder.DropColumn(
                name: "Enlem",
                table: "DevamKayitlari");
        }
    }
}
