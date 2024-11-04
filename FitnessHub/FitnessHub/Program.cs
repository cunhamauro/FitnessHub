using FitnessHub.Data;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Syncfusion.Licensing;
using System.Text;

namespace FitnessHub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            IConfiguration configuration = builder.Configuration;

            builder.Services.AddIdentity<User, IdentityRole>(cfg =>
            {
                // Token Configuration
                cfg.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
                cfg.SignIn.RequireConfirmedEmail = true;

                // Password Configuration
                cfg.User.RequireUniqueEmail = true;
                cfg.Password.RequireDigit = true;
                cfg.Password.RequiredUniqueChars = 1;
                cfg.Password.RequireUppercase = true;
                cfg.Password.RequireLowercase = true;
                cfg.Password.RequireNonAlphanumeric = false;
                cfg.Password.RequiredLength = 8;

                // Lockout Configuration
                cfg.Lockout.MaxFailedAccessAttempts = 3;
                cfg.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                cfg.Lockout.AllowedForNewUsers = true;
            })
             .AddDefaultTokenProviders()
             .AddEntityFrameworkStores<DataContext>();

            SyncfusionLicenseProvider.RegisterLicense(configuration[@"Syncfusion:LicenseKey"]);

            builder.Services.AddAuthentication()
                .AddCookie()
                .AddJwtBearer(cfg =>
                {
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = configuration["Tokens:Issuer"],
                        ValidAudience = configuration["Tokens:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["Tokens:Key"]))
                    };
                });

            builder.Services.AddDbContext<DataContext>(config => config.UseSqlServer("name=LocalConnection"));

            builder.Services.AddTransient<SeedDb>();

            // Helpers
            builder.Services.AddScoped<IUserHelper, UserHelper>();
            builder.Services.AddScoped<IMailHelper, MailHelper>();
            builder.Services.AddScoped<IImageHelper, ImageHelper>();
            builder.Services.AddScoped<ILoadRolesHelper, LoadRolesHelper>();
            builder.Services.AddScoped<IConverterHelper,  ConverterHelper>();

            // Repositories
            builder.Services.AddScoped<IClassRepository, ClassRepository>();
            builder.Services.AddScoped<IMachineCategoryRepository, MachineCategoryRepository>();
            builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
            builder.Services.AddScoped<IGymRepository, GymRepository>();
            builder.Services.AddScoped<IClassRepository, ClassRepository>();
            builder.Services.AddScoped<IMachineRepository, MachineRepository>();
            builder.Services.AddScoped<IMembershipRepository, MembershipRepository>();
            builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();
            builder.Services.AddScoped<IClassCategoryRepository, ClassCategoryRepository>();
            builder.Services.AddScoped<IMachineDetailsRepository, MachineDetailsRepository>();
            builder.Services.AddScoped<IMembershipDetailsRepository, MembershipDetailsRepository>();
            builder.Services.AddScoped<IClassHistoryRepository, ClassHistoryRepository>();
            builder.Services.AddScoped<IRequestInstructorRepository, RequestInstructorRepository>();
            builder.Services.AddScoped<IClientInstructorAppointmentRepository, ClientInstructorAppointmentRepository>();
            builder.Services.AddScoped<IRequestInstructorHistoryRepository, RequestInstructorHistoryRepository>();
            builder.Services.AddScoped<IClientInstructorAppointmentHistoryRepository, ClientInstructorAppointmentHistoryRepository>();
            builder.Services.AddScoped<IClientMembershipHistoryRepository, ClientMembershipHistoryRepository>();
            builder.Services.AddScoped<IMembershipHistoryRepository, MembershipHistoryRepository>();


            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/NotAuthorized";
                options.AccessDeniedPath = "/Account/NotAuthorized";
            });

            // Api services
            builder.Services.AddHttpClient<CountryService>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<SeedDb>();
                seeder.SeedAsync().Wait();
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Home/PageNotFound");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            // Middleware to force the logout if the user doesn't exist in the database
            app.Use(async (context, next) =>
            {
                if (context.User?.Identity?.IsAuthenticated == true)
                {
                    var userManager = context.RequestServices.GetRequiredService<UserManager<User>>();
                    var user = await userManager.GetUserAsync(context.User);

                    if (user == null)
                    {
                        // User doesn't exist, the logout is forced
                        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
                        context.Response.Redirect("/Account/Login");
                        return;
                    }
                }

                await next();
            });

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
