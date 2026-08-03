using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajyerTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EkDosyaAlanlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EkDosyaAdi",
                table: "Talepler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EkDosyaOrijinalAdi",
                table: "Talepler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EkDosyaAdi",
                table: "Gorevler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EkDosyaOrijinalAdi",
                table: "Gorevler",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EkDosyaAdi",
                table: "Talepler");

            migrationBuilder.DropColumn(
                name: "EkDosyaOrijinalAdi",
                table: "Talepler");

            migrationBuilder.DropColumn(
                name: "EkDosyaAdi",
                table: "Gorevler");

            migrationBuilder.DropColumn(
                name: "EkDosyaOrijinalAdi",
                table: "Gorevler");
        }
    }
}
