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

        public DbSet<ClassType> ClassTypes { get; set; }

        public DbSet<ClassWaitlist> ClassWaitlists { get; set; }

        // Machines

        public DbSet<Machine> Machines { get; set; }

        public DbSet<MachineCategory> MachineCategories { get; set; }

        public DbSet<Exercise> Exercises { get; set; }

        public DbSet<MachineDetails> MachineDetails { get; set; }

        public DbSet<Workout> Workouts { get; set; }

        // Communication

        public DbSet<RequestInstructor> RequestsIntructor { get; set; }

        public DbSet<ClientInstructorAppointment> ClientInstructorAppointments { get; set; }

        public DbSet<ForumThread> ForumThreads { get; set; }

        public DbSet<ForumReply> ForumReplies { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<TicketMessage> TicketMessages { get; set; }

        // History

        public DbSet<RequestInstructorHistory> RequestsIntructorHistory { get; set; }

        public DbSet<ClientInstructorAppointmentHistory> ClientInstructorAppointmentsHistory { get; set; }

        public DbSet<ClientMembershipHistory> ClientMembershipHistory { get; set; }

        public DbSet<ClassHistory> ClassHistory { get; set; }

        public DbSet<GymHistory> GymsHistory { get; set; }

        public DbSet<ClientHistory> ClientsHistory { get; set; }

        public DbSet<StaffHistory> StaffHistory { get; set; }

        public DbSet<MembershipHistory> MembershipHistory { get; set; }

        public DbSet<WeightProgress> WeightProgress { get; set; }

        public DbSet<RegisteredInClassesHistory> ClassesRegistrationHistory { get; set; }

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
                       .Property(m => m.MonthlyFee)
                       .HasColumnType("decimal(18,2)");

            builder.Entity<MembershipHistory>()
                       .Property(m => m.Price)
                       .HasColumnType("decimal(18,2)");

            builder.Entity<Instructor>()
                       .Property(m => m.Rating)
                       .HasColumnType("decimal(18,2)");

            builder.Entity<ClassType>()
                       .Property(m => m.Rating)
                       .HasColumnType("decimal(18,2)");

            builder.Entity<RegisteredInClassesHistory>()
                       .Property(m => m.Rating)
                       .HasColumnType("decimal(18,2)");

            // Disable database ID generation
            builder.Entity<ClassHistory>()
                       .Property(e => e.Id)
                       .ValueGeneratedNever();

            builder.Entity<RequestInstructorHistory>()
                       .Property(e => e.Id)
                       .ValueGeneratedNever();

            builder.Entity<ClientInstructorAppointmentHistory>()
                       .Property(e => e.Id)
                       .ValueGeneratedNever();

            builder.Entity<MembershipHistory>()
                       .Property(e => e.Id)
                       .ValueGeneratedNever();

            builder.Entity<ClientMembershipHistory>()
                       .Property(e => e.Id)
                       .ValueGeneratedNever();

            builder.Entity<GymHistory>()
                       .Property(e => e.Id)
                       .ValueGeneratedNever();

            builder.Entity<ClassWaitlist>()
                       .Property(e => e.Id)
                       .ValueGeneratedNever();

            base.OnModelCreating(builder);
        }
    }
}
