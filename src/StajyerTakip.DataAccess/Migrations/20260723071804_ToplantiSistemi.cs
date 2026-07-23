using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajyerTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ToplantiSistemi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Toplantilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MentorId = table.Column<int>(type: "int", nullable: false),
                    Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Toplantilar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Toplantilar_Mentorler_MentorId",
                        column: x => x.MentorId,
                        principalTable: "Mentorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ToplantiKatilimlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToplantiId = table.Column<int>(type: "int", nullable: false),
                    StajyerId = table.Column<int>(type: "int", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RetSebebi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CevapTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToplantiKatilimlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToplantiKatilimlari_Stajyerler_StajyerId",
                        column: x => x.StajyerId,
                        principalTable: "Stajyerler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToplantiKatilimlari_Toplantilar_ToplantiId",
                        column: x => x.ToplantiId,
                        principalTable: "Toplantilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToplantiKatilimlari_StajyerId",
                table: "ToplantiKatilimlari",
                column: "StajyerId");

            migrationBuilder.CreateIndex(
                name: "IX_ToplantiKatilimlari_ToplantiId",
                table: "ToplantiKatilimlari",
                column: "ToplantiId");

            migrationBuilder.CreateIndex(
                name: "IX_Toplantilar_MentorId",
                table: "Toplantilar",
                column: "MentorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToplantiKatilimlari");

            migrationBuilder.DropTable(
                name: "Toplantilar");
        }
    }
}
