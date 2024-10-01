using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessHub.Migrations
{
    /// <inheritdoc />
    public partial class membershipDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MachineDetails_Gyms_GymId",
                table: "MachineDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_MachineDetails_Machines_MachineId",
                table: "MachineDetails");

            migrationBuilder.DropColumn(
                name: "DateRenewal",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Memberships");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Memberships",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "MachineId",
                table: "MachineDetails",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "GymId",
                table: "MachineDetails",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "MembershipDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    DateRenewal = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipDetails", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_MachineDetails_Gyms_GymId",
                table: "MachineDetails",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MachineDetails_Machines_MachineId",
                table: "MachineDetails",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MachineDetails_Gyms_GymId",
                table: "MachineDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_MachineDetails_Machines_MachineId",
                table: "MachineDetails");

            migrationBuilder.DropTable(
                name: "MembershipDetails");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Memberships");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateRenewal",
                table: "Memberships",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Memberships",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "MachineId",
                table: "MachineDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GymId",
                table: "MachineDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MachineDetails_Gyms_GymId",
                table: "MachineDetails",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MachineDetails_Machines_MachineId",
                table: "MachineDetails",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
