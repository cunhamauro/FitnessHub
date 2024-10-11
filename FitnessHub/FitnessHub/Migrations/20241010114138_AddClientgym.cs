using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessHub.Migrations
{
    /// <inheritdoc />
    public partial class AddClientgym : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GymId",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_GymId",
                table: "Clients",
                column: "GymId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Gyms_GymId",
                table: "Clients",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Gyms_GymId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_GymId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "GymId",
                table: "Clients");
        }
    }
}
