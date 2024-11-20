using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessHub.Migrations
{
    /// <inheritdoc />
    public partial class ProgressProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Progress",
                table: "WeightProgress",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Progress",
                table: "WeightProgress");
        }
    }
}
