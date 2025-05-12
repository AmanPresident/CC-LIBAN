using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace test7.Migrations
{
    /// <inheritdoc />
    public partial class AfficherCategorieDansProduit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategorieId",
                table: "Produits",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produits_CategorieId",
                table: "Produits",
                column: "CategorieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Produits_Categorie_CategorieId",
                table: "Produits",
                column: "CategorieId",
                principalTable: "Categorie",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produits_Categorie_CategorieId",
                table: "Produits");

            migrationBuilder.DropIndex(
                name: "IX_Produits_CategorieId",
                table: "Produits");

            migrationBuilder.DropColumn(
                name: "CategorieId",
                table: "Produits");
        }
    }
}
