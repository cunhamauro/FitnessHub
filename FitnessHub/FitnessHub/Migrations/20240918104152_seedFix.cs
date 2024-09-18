using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessHub.Migrations
{
    /// <inheritdoc />
    public partial class seedFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_Gyms_GymId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientGymClass_Clients_ListClientsId",
                table: "ClientGymClass");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Memberships_MembershipId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Gyms_GymId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Instructors_Gyms_GymId",
                table: "Instructors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientGymClass",
                table: "ClientGymClass");

            migrationBuilder.DropIndex(
                name: "IX_ClientGymClass_ListClientsId",
                table: "ClientGymClass");

            migrationBuilder.RenameColumn(
                name: "ListClientsId",
                table: "ClientGymClass",
                newName: "ClientId");

            migrationBuilder.AlterColumn<int>(
                name: "GymId",
                table: "Instructors",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "GymId",
                table: "Employees",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MembershipId",
                table: "Clients",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "GymId",
                table: "Admins",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientGymClass",
                table: "ClientGymClass",
                columns: new[] { "ClientId", "GymClassId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientGymClass_GymClassId",
                table: "ClientGymClass",
                column: "GymClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_Gyms_GymId",
                table: "Admins",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientGymClass_Clients_ClientId",
                table: "ClientGymClass",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Memberships_MembershipId",
                table: "Clients",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Gyms_GymId",
                table: "Employees",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Instructors_Gyms_GymId",
                table: "Instructors",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_Gyms_GymId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientGymClass_Clients_ClientId",
                table: "ClientGymClass");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Memberships_MembershipId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Gyms_GymId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Instructors_Gyms_GymId",
                table: "Instructors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientGymClass",
                table: "ClientGymClass");

            migrationBuilder.DropIndex(
                name: "IX_ClientGymClass_GymClassId",
                table: "ClientGymClass");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "ClientGymClass",
                newName: "ListClientsId");

            migrationBuilder.AlterColumn<int>(
                name: "GymId",
                table: "Instructors",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GymId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MembershipId",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GymId",
                table: "Admins",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientGymClass",
                table: "ClientGymClass",
                columns: new[] { "GymClassId", "ListClientsId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientGymClass_ListClientsId",
                table: "ClientGymClass",
                column: "ListClientsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_Gyms_GymId",
                table: "Admins",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientGymClass_Clients_ListClientsId",
                table: "ClientGymClass",
                column: "ListClientsId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Memberships_MembershipId",
                table: "Clients",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Gyms_GymId",
                table: "Employees",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Instructors_Gyms_GymId",
                table: "Instructors",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
