using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.GymClasses;
using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Entities.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using FitnessHub.Models;

namespace FitnessHub.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DbSet<Gym> Gyms { get; set; }

        // Users

        public DbSet<Admin> Admins { get; set; } // Roles: Admin & MasterAdmin

        public DbSet<Client> Clients { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Instructor> Instructors { get; set; }

        public DbSet<Membership> Memberships { get; set; }

        public DbSet<MembershipDetails> MembershipDetails { get; set; }

        // Classes

        public DbSet<GymClass> GymClasses {  get; set; }

        public DbSet<OnlineClass> OnlineClasses { get; set; }

        public DbSet<VideoClass> VideoClasses { get; set; }

        // Machines

        public DbSet<Machine> Machines { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Exercise> Exercises { get; set; }

        public DbSet<MachineDetail> MachineDetails { get; set; }

        public DbSet<Workout> Workouts { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // TPC (Table Per Class) Inheritance Configuration to separate different User Classes
            builder.Entity<User>().ToTable("AspNetUsers");
            builder.Entity<Admin>().ToTable("Admins");
            builder.Entity<Client>().ToTable("Clients");
            builder.Entity<Employee>().ToTable("Employees");
            builder.Entity<Instructor>().ToTable("Instructors");

            // Make price with two decimals
            builder.Entity<Membership>()
                       .Property(m => m.Price)
                       .HasColumnType("decimal(18,2)");

            base.OnModelCreating(builder);
        }
        
    }
}
