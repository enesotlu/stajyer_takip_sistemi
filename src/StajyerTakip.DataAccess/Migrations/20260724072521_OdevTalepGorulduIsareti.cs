using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajyerTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OdevTalepGorulduIsareti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Degerlendirmeler");

            migrationBuilder.AddColumn<bool>(
                name: "MentorGordu",
                table: "Talepler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MentorGordu",
                table: "Gorevler",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MentorGordu",
                table: "Talepler");

            migrationBuilder.DropColumn(
                name: "MentorGordu",
                table: "Gorevler");

            migrationBuilder.CreateTable(
                name: "Degerlendirmeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MentorId = table.Column<int>(type: "int", nullable: false),
                    StajyerId = table.Column<int>(type: "int", nullable: false),
                    Puan = table.Column<int>(type: "int", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Yorum = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Degerlendirmeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Degerlendirmeler_Mentorler_MentorId",
                        column: x => x.MentorId,
                        principalTable: "Mentorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Degerlendirmeler_Stajyerler_StajyerId",
                        column: x => x.StajyerId,
                        principalTable: "Stajyerler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Degerlendirmeler_MentorId",
                table: "Degerlendirmeler",
                column: "MentorId");

            migrationBuilder.CreateIndex(
                name: "IX_Degerlendirmeler_StajyerId",
                table: "Degerlendirmeler",
                column: "StajyerId");
        }
    }
}
