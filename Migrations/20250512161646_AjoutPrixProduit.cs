using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace test7.Migrations
{
    /// <inheritdoc />
    public partial class AjoutPrixProduit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Prix",
                table: "Produits",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prix",
                table: "Produits");
        }
    }
}
