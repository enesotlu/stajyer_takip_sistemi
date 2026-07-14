using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajyerTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departmanlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departmanlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Duyurular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icerik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YayinTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Duyurular", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mentorler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unvan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartmanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mentorler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mentorler_Departmanlar_DepartmanId",
                        column: x => x.DepartmanId,
                        principalTable: "Departmanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stajyerler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Okul = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bolum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MentorId = table.Column<int>(type: "int", nullable: false),
                    DepartmanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stajyerler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stajyerler_Departmanlar_DepartmanId",
                        column: x => x.DepartmanId,
                        principalTable: "Departmanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stajyerler_Mentorler_MentorId",
                        column: x => x.MentorId,
                        principalTable: "Mentorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Degerlendirmeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StajyerId = table.Column<int>(type: "int", nullable: false),
                    MentorId = table.Column<int>(type: "int", nullable: false),
                    Puan = table.Column<int>(type: "int", nullable: false),
                    Yorum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "DevamKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StajyerId = table.Column<int>(type: "int", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GirisSaati = table.Column<TimeSpan>(type: "time", nullable: true),
                    CikisSaati = table.Column<TimeSpan>(type: "time", nullable: true),
                    OnayDurumu = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevamKayitlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevamKayitlari_Stajyerler_StajyerId",
                        column: x => x.StajyerId,
                        principalTable: "Stajyerler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Gorevler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StajyerId = table.Column<int>(type: "int", nullable: false),
                    Baslik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeslimTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gorevler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gorevler_Stajyerler_StajyerId",
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

            migrationBuilder.CreateIndex(
                name: "IX_DevamKayitlari_StajyerId",
                table: "DevamKayitlari",
                column: "StajyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Gorevler_StajyerId",
                table: "Gorevler",
                column: "StajyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Mentorler_DepartmanId",
                table: "Mentorler",
                column: "DepartmanId");

            migrationBuilder.CreateIndex(
                name: "IX_Stajyerler_DepartmanId",
                table: "Stajyerler",
                column: "DepartmanId");

            migrationBuilder.CreateIndex(
                name: "IX_Stajyerler_MentorId",
                table: "Stajyerler",
                column: "MentorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Degerlendirmeler");

            migrationBuilder.DropTable(
                name: "DevamKayitlari");

            migrationBuilder.DropTable(
                name: "Duyurular");

            migrationBuilder.DropTable(
                name: "Gorevler");

            migrationBuilder.DropTable(
                name: "Stajyerler");

            migrationBuilder.DropTable(
                name: "Mentorler");

            migrationBuilder.DropTable(
                name: "Departmanlar");
        }
    }
}
