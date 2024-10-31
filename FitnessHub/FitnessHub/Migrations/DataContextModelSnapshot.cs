﻿// <auto-generated />
using System;
using FitnessHub.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FitnessHub.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ClientGymClass", b =>
                {
                    b.Property<string>("ClientsId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("GymClassId")
                        .HasColumnType("int");

                    b.HasKey("ClientsId", "GymClassId");

                    b.HasIndex("GymClassId");

                    b.ToTable("ClientGymClass");
                });

            modelBuilder.Entity("ClientOnlineClass", b =>
                {
                    b.Property<string>("ClientsId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("OnlineClassId")
                        .HasColumnType("int");

                    b.HasKey("ClientsId", "OnlineClassId");

                    b.HasIndex("OnlineClassId");

                    b.ToTable("ClientOnlineClass");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.Gym", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("NumReviews")
                        .HasColumnType("int");

                    b.Property<int>("Rating")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Gyms");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymClasses.Class", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CategoryId")
                        .HasColumnType("int");

                    b.Property<int?>("NumReviews")
                        .HasColumnType("int");

                    b.Property<int?>("Rating")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Classes", (string)null);

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymClasses.ClassCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ClassCategories");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymMachines.Exercise", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("DayOfWeek")
                        .HasColumnType("int");

                    b.Property<int?>("MachineId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Repetitions")
                        .HasColumnType("int");

                    b.Property<int>("Sets")
                        .HasColumnType("int");

                    b.Property<long>("Ticks")
                        .HasColumnType("bigint");

                    b.Property<int?>("WorkoutId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("MachineId");

                    b.HasIndex("WorkoutId");

                    b.ToTable("Exercises");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymMachines.Machine", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("ImagePath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TutorialVideoUrl")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Machines");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymMachines.MachineCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("MachineCategories");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymMachines.MachineDetails", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("GymId")
                        .HasColumnType("int");

                    b.Property<int?>("MachineId")
                        .HasColumnType("int");

                    b.Property<int>("MachineNumber")
                        .HasColumnType("int");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("GymId");

                    b.HasIndex("MachineId");

                    b.ToTable("MachineDetails");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymMachines.Workout", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClientId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("InstructorId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.HasIndex("InstructorId");

                    b.ToTable("Workouts");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.History.ClassHistory", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<bool>("Canceled")
                        .HasColumnType("bit");

                    b.Property<int?>("Capacity")
                        .HasColumnType("int");

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("ClassType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("DateEnd")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateStart")
                        .HasColumnType("datetime2");

                    b.Property<int?>("GymId")
                        .HasColumnType("int");

                    b.Property<string>("InstructorId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Platform")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("VideoClassUrl")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ClassHistory");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.Users.Membership", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.ToTable("Memberships");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.Users.MembershipDetails", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DateRenewal")
                        .HasColumnType("datetime2");

                    b.Property<int?>("MembershipId")
                        .HasColumnType("int");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("MembershipId");

                    b.ToTable("MembershipDetails");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.Users.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("BirthDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FirstName")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("ImagePath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymClasses.GymClass", b =>
                {
                    b.HasBaseType("FitnessHub.Data.Entities.GymClasses.Class");

                    b.Property<int>("Capacity")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateEnd")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateStart")
                        .HasColumnType("datetime2");

                    b.Property<int?>("GymId")
                        .HasColumnType("int");

                    b.Property<string>("InstructorId")
                        .HasColumnType("nvarchar(450)");

                    b.HasIndex("GymId");

                    b.HasIndex("InstructorId");

                    b.ToTable("GymClasses", (string)null);
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymClasses.OnlineClass", b =>
                {
                    b.HasBaseType("FitnessHub.Data.Entities.GymClasses.Class");

                    b.Property<DateTime>("DateEnd")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateStart")
                        .HasColumnType("datetime2");

                    b.Property<string>("InstructorId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Platform")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasIndex("InstructorId");

                    b.ToTable("OnlineClasses", (string)null);
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymClasses.VideoClass", b =>
                {
                    b.HasBaseType("FitnessHub.Data.Entities.GymClasses.Class");

                    b.Property<string>("VideoClassUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("VideoClasses", (string)null);
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.Users.Admin", b =>
                {
                    b.HasBaseType("FitnessHub.Data.Entities.Users.User");

                    b.Property<int?>("GymId")
                        .HasColumnType("int");

                    b.HasIndex("GymId");

                    b.ToTable("Admins", (string)null);
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.Users.Client", b =>
                {
                    b.HasBaseType("FitnessHub.Data.Entities.Users.User");

                    b.Property<string>("FullAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("GymId")
                        .HasColumnType("int");

                    b.Property<string>("IdentificationNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("MembershipDetailsId")
                        .HasColumnType("int");

                    b.HasIndex("GymId");

                    b.HasIndex("MembershipDetailsId");

                    b.ToTable("Clients", (string)null);
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.Users.Employee", b =>
                {
                    b.HasBaseType("FitnessHub.Data.Entities.Users.User");

                    b.Property<int?>("GymId")
                        .IsRequired()
                        .HasColumnType("int");

                    b.HasIndex("GymId");

                    b.ToTable("Employees", (string)null);
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.Users.Instructor", b =>
                {
                    b.HasBaseType("FitnessHub.Data.Entities.Users.User");

                    b.Property<int?>("GymId")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<int>("NumReviews")
                        .HasColumnType("int");

                    b.Property<int>("Rating")
                        .HasColumnType("int");

                    b.HasIndex("GymId");

                    b.ToTable("Instructors", (string)null);
                });

            modelBuilder.Entity("ClientGymClass", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.Users.Client", null)
                        .WithMany()
                        .HasForeignKey("ClientsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FitnessHub.Data.Entities.GymClasses.GymClass", null)
                        .WithMany()
                        .HasForeignKey("GymClassId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ClientOnlineClass", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.Users.Client", null)
                        .WithMany()
                        .HasForeignKey("ClientsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FitnessHub.Data.Entities.GymClasses.OnlineClass", null)
                        .WithMany()
                        .HasForeignKey("OnlineClassId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymClasses.Class", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.GymClasses.ClassCategory", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymMachines.Exercise", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.GymMachines.Machine", "Machine")
                        .WithMany()
                        .HasForeignKey("MachineId");

                    b.HasOne("FitnessHub.Data.Entities.GymMachines.Workout", null)
                        .WithMany("Exercises")
                        .HasForeignKey("WorkoutId");

                    b.Navigation("Machine");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymMachines.Machine", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.GymMachines.MachineCategory", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymMachines.MachineDetails", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.Gym", "Gym")
                        .WithMany()
                        .HasForeignKey("GymId");

                    b.HasOne("FitnessHub.Data.Entities.GymMachines.Machine", "Machine")
                        .WithMany()
                        .HasForeignKey("MachineId");

                    b.Navigation("Gym");

                    b.Navigation("Machine");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymMachines.Workout", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.Users.Client", "Client")
                        .WithMany()
                        .HasForeignKey("ClientId");

                    b.HasOne("FitnessHub.Data.Entities.Users.Instructor", "Instructor")
                        .WithMany()
                        .HasForeignKey("InstructorId");

                    b.Navigation("Client");

                    b.Navigation("Instructor");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.Users.MembershipDetails", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.Users.Membership", "Membership")
                        .WithMany()
                        .HasForeignKey("MembershipId");

                    b.Navigation("Membership");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.Users.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.Users.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FitnessHub.Data.Entities.Users.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.Users.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymClasses.GymClass", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.Gym", "Gym")
                        .WithMany()
                        .HasForeignKey("GymId");

                    b.HasOne("FitnessHub.Data.Entities.GymClasses.Class", null)
                        .WithOne()
                        .HasForeignKey("FitnessHub.Data.Entities.GymClasses.GymClass", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FitnessHub.Data.Entities.Users.Instructor", "Instructor")
                        .WithMany()
                        .HasForeignKey("InstructorId");

                    b.Navigation("Gym");

                    b.Navigation("Instructor");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymClasses.OnlineClass", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.GymClasses.Class", null)
                        .WithOne()
                        .HasForeignKey("FitnessHub.Data.Entities.GymClasses.OnlineClass", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FitnessHub.Data.Entities.Users.Instructor", "Instructor")
                        .WithMany()
                        .HasForeignKey("InstructorId");

                    b.Navigation("Instructor");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymClasses.VideoClass", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.GymClasses.Class", null)
                        .WithOne()
                        .HasForeignKey("FitnessHub.Data.Entities.GymClasses.VideoClass", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.Users.Admin", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.Gym", "Gym")
                        .WithMany()
                        .HasForeignKey("GymId");

                    b.HasOne("FitnessHub.Data.Entities.Users.User", null)
                        .WithOne()
                        .HasForeignKey("FitnessHub.Data.Entities.Users.Admin", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Gym");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.Users.Client", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.Gym", "Gym")
                        .WithMany()
                        .HasForeignKey("GymId");

                    b.HasOne("FitnessHub.Data.Entities.Users.User", null)
                        .WithOne()
                        .HasForeignKey("FitnessHub.Data.Entities.Users.Client", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FitnessHub.Data.Entities.Users.MembershipDetails", "MembershipDetails")
                        .WithMany()
                        .HasForeignKey("MembershipDetailsId");

                    b.Navigation("Gym");

                    b.Navigation("MembershipDetails");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.Users.Employee", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.Gym", "Gym")
                        .WithMany()
                        .HasForeignKey("GymId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FitnessHub.Data.Entities.Users.User", null)
                        .WithOne()
                        .HasForeignKey("FitnessHub.Data.Entities.Users.Employee", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Gym");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.Users.Instructor", b =>
                {
                    b.HasOne("FitnessHub.Data.Entities.Gym", "Gym")
                        .WithMany()
                        .HasForeignKey("GymId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FitnessHub.Data.Entities.Users.User", null)
                        .WithOne()
                        .HasForeignKey("FitnessHub.Data.Entities.Users.Instructor", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Gym");
                });

            modelBuilder.Entity("FitnessHub.Data.Entities.GymMachines.Workout", b =>
                {
                    b.Navigation("Exercises");
                });
#pragma warning restore 612, 618
        }
    }
}
