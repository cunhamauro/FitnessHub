using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.Communication;
using FitnessHub.Data.Entities.GymClasses;
using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Entities.History;
using FitnessHub.Data.Entities.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DbSet<Gym> Gyms { get; set; }

        // Users

        public DbSet<Admin> Admins { get; set; } // Roles: Admin & MasterAdmin

        public DbSet<Client> Clients { get; set; }

        public DbSet<Employee> Employees { get; set; }

        //public DbSet<ClassDetails> ClassDetails { get; set; }

        public DbSet<Instructor> Instructors { get; set; }

        public DbSet<Membership> Memberships { get; set; }

        public DbSet<MembershipDetails> MembershipDetails { get; set; }

        // Classes

        public DbSet<Class> Class { get; set; }

        public DbSet<GymClass> GymClasses { get; set; }

        public DbSet<OnlineClass> OnlineClasses { get; set; }

        public DbSet<VideoClass> VideoClasses { get; set; }

        public DbSet<ClassCategory> ClassCategories { get; set; }

        // Machines

        public DbSet<Machine> Machines { get; set; }

        public DbSet<MachineCategory> MachineCategories { get; set; }

        public DbSet<Exercise> Exercises { get; set; }

        public DbSet<MachineDetails> MachineDetails { get; set; }

        public DbSet<Workout> Workouts { get; set; }

        // Communication

        public DbSet<RequestInstructor> RequestsIntructor { get; set; }

        public DbSet<ClientInstructorAppointment> ClientInstructorAppointments { get; set; }

        // History

        public DbSet<RequestInstructorHistory> RequestsIntructorHistory { get; set; }

        public DbSet<ClientInstructorAppointmentHistory> ClientInstructorAppointmentsHistory { get; set; }

        public DbSet<ClassHistory> ClassHistory { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Table-per-Type (TPT) Inheritance Configuration to separate different sub classes
            builder.Entity<User>().ToTable("AspNetUsers");
            builder.Entity<Admin>().ToTable("Admins");
            builder.Entity<Client>().ToTable("Clients");
            builder.Entity<Employee>().ToTable("Employees");
            builder.Entity<Instructor>().ToTable("Instructors");

            builder.Entity<Class>().ToTable("Classes");
            builder.Entity<GymClass>().ToTable("GymClasses").HasBaseType<Class>();
            builder.Entity<OnlineClass>().ToTable("OnlineClasses").HasBaseType<Class>();
            builder.Entity<VideoClass>().ToTable("VideoClasses").HasBaseType<Class>();

            // Make price with two decimals
            builder.Entity<Membership>()
                       .Property(m => m.Price)
                       .HasColumnType("decimal(18,2)");

            // Disable database ID generation
            builder.Entity<RequestInstructorHistory>()
                       .Property(e => e.Id)
                       .ValueGeneratedNever();

            builder.Entity<ClientInstructorAppointmentHistory>()
                       .Property(e => e.Id)
                       .ValueGeneratedNever();

            builder.Entity<ClassHistory>()
                       .Property(e => e.Id)
                       .ValueGeneratedNever();

            base.OnModelCreating(builder);
        }
    }
}
