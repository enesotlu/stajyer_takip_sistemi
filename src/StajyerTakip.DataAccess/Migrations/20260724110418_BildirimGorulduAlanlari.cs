using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajyerTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class BildirimGorulduAlanlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StajyerGordu",
                table: "ToplantiKatilimlari",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "StajyerGordu",
                table: "Talepler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MentorGordu",
                table: "Izinler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "StajyerGordu",
                table: "Gorevler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MentorGordu",
                table: "DevamKayitlari",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BasvuruGorulduMu",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StajyerGordu",
                table: "ToplantiKatilimlari");

            migrationBuilder.DropColumn(
                name: "StajyerGordu",
                table: "Talepler");

            migrationBuilder.DropColumn(
                name: "MentorGordu",
                table: "Izinler");

            migrationBuilder.DropColumn(
                name: "StajyerGordu",
                table: "Gorevler");

            migrationBuilder.DropColumn(
                name: "MentorGordu",
                table: "DevamKayitlari");

            migrationBuilder.DropColumn(
                name: "BasvuruGorulduMu",
                table: "AspNetUsers");
        }
    }
}
