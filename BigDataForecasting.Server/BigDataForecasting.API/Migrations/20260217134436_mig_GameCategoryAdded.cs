using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigDataForecasting.API.Migrations
{
    /// <inheritdoc />
    public partial class mig_GameCategoryAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameCategories",
                columns: table => new
                {
                    GameCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameCategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCategories", x => x.GameCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "GameCategoryMappings",
                columns: table => new
                {
                    GameCategoriesGameCategoryId = table.Column<int>(type: "int", nullable: false),
                    GamesGameId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCategoryMappings", x => new { x.GameCategoriesGameCategoryId, x.GamesGameId });
                    table.ForeignKey(
                        name: "FK_GameCategoryMappings_GameCategories_GameCategoriesGameCategoryId",
                        column: x => x.GameCategoriesGameCategoryId,
                        principalTable: "GameCategories",
                        principalColumn: "GameCategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameCategoryMappings_Games_GamesGameId",
                        column: x => x.GamesGameId,
                        principalTable: "Games",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameCategoryMappings_GamesGameId",
                table: "GameCategoryMappings",
                column: "GamesGameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameCategoryMappings");

            migrationBuilder.DropTable(
                name: "GameCategories");
        }
    }
}
