using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajyerTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class GorevTeslimAlanlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MentorNotu",
                table: "Gorevler",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeslimDosyaAdi",
                table: "Gorevler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeslimDosyaOrijinalAdi",
                table: "Gorevler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TeslimEdilmeTarihi",
                table: "Gorevler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeslimNotu",
                table: "Gorevler",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MentorNotu",
                table: "Gorevler");

            migrationBuilder.DropColumn(
                name: "TeslimDosyaAdi",
                table: "Gorevler");

            migrationBuilder.DropColumn(
                name: "TeslimDosyaOrijinalAdi",
                table: "Gorevler");

            migrationBuilder.DropColumn(
                name: "TeslimEdilmeTarihi",
                table: "Gorevler");

            migrationBuilder.DropColumn(
                name: "TeslimNotu",
                table: "Gorevler");
        }
    }
}
