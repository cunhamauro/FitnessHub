using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessHub.Migrations
{
    /// <inheritdoc />
    public partial class classesControllerMshipFixV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MembershipId",
                table: "MembershipDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipDetails_MembershipId",
                table: "MembershipDetails",
                column: "MembershipId");

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipDetails_Memberships_MembershipId",
                table: "MembershipDetails",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipDetails_Memberships_MembershipId",
                table: "MembershipDetails");

            migrationBuilder.DropIndex(
                name: "IX_MembershipDetails_MembershipId",
                table: "MembershipDetails");

            migrationBuilder.DropColumn(
                name: "MembershipId",
                table: "MembershipDetails");
        }
    }
}
