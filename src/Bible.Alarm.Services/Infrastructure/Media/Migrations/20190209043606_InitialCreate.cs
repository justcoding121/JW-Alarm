using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bible.Alarm.Services.Infrastructure.Media.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BibleTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true),
                    LanguageId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BibleTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BibleTranslations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SongBooks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true),
                    LanguageId = table.Column<int>(nullable: false),
                    MusicType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongBooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SongBooks_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BibleBook",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Number = table.Column<int>(nullable: false),
                    BibleTranslationId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BibleBook", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BibleBook_BibleTranslations_BibleTranslationId",
                        column: x => x.BibleTranslationId,
                        principalTable: "BibleTranslations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MusicTrack",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Duration = table.Column<TimeSpan>(nullable: false),
                    Url = table.Column<string>(nullable: true),
                    SongBookId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicTrack", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MusicTrack_SongBooks_SongBookId",
                        column: x => x.SongBookId,
                        principalTable: "SongBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BibleChapter",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<int>(nullable: false),
                    Url = table.Column<string>(nullable: true),
                    Duration = table.Column<TimeSpan>(nullable: false),
                    BibleBookId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BibleChapter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BibleChapter_BibleBook_BibleBookId",
                        column: x => x.BibleBookId,
                        principalTable: "BibleBook",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BibleBook_BibleTranslationId",
                table: "BibleBook",
                column: "BibleTranslationId");

            migrationBuilder.CreateIndex(
                name: "IX_BibleChapter_BibleBookId",
                table: "BibleChapter",
                column: "BibleBookId");

            migrationBuilder.CreateIndex(
                name: "IX_BibleTranslations_LanguageId",
                table: "BibleTranslations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicTrack_SongBookId",
                table: "MusicTrack",
                column: "SongBookId");

            migrationBuilder.CreateIndex(
                name: "IX_SongBooks_LanguageId",
                table: "SongBooks",
                column: "LanguageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BibleChapter");

            migrationBuilder.DropTable(
                name: "MusicTrack");

            migrationBuilder.DropTable(
                name: "BibleBook");

            migrationBuilder.DropTable(
                name: "SongBooks");

            migrationBuilder.DropTable(
                name: "BibleTranslations");

            migrationBuilder.DropTable(
                name: "Languages");
        }
    }
}
