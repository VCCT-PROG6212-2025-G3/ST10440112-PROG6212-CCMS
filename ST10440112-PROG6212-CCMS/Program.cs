using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Filters;
using ST10440112_PROG6212_CCMS.Models;
using ST10440112_PROG6212_CCMS.Services;

namespace ST10440112_PROG6212_CCMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews(options =>
            {
                // Add the role-based access filter globally
                options.Filters.Add<RoleBasedAccessFilter>();
            });

            // Add db Context
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add Identity
            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Register File Upload Service
            builder.Services.AddScoped<IFileUploadService, FileUploadService>();

            // Register File Encryption Service
            builder.Services.AddScoped<IFileEncryptionService, FileEncryptionService>();

            // Register Session Management Service
            builder.Services.AddScoped<ISessionManagementService, SessionManagementService>();

            // Register Claim Validation Service
            builder.Services.AddScoped<IClaimValidationService, ClaimValidationService>();

            // Register Login Redirect Helper
            builder.Services.AddScoped<ILoginRedirectHelper, LoginRedirectHelper>();

            // Add session support
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline with enhanced error handling
            if (app.Environment.IsDevelopment())
            {
                // Development: Show detailed error pages
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Production: Use custom error handler
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Handle specific status codes (404, 403, 500)
            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Seed roles and users
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                SeedRolesAndUsers(roleManager, userManager).GetAwaiter().GetResult();
            }

            app.Run();
        }

        private static async Task SeedRolesAndUsers(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            // Create roles
            string[] roleNames = { "Lecturer", "ProgrammeCoordinator", "AcademicManager", "HR" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create users
            // Lecturer: Michael Jones
            var lecturerUser = await userManager.FindByEmailAsync("michael.jones@newlands.ac.za");
            if (lecturerUser == null)
            {
                lecturerUser = new AppUser
                {
                    UserName = "michael.jones",
                    Email = "michael.jones@newlands.ac.za",
                    FullName = "Michael Jones",
                    Role = "Lecturer"
                };
                await userManager.CreateAsync(lecturerUser, "Password123");
                await userManager.AddToRoleAsync(lecturerUser, "Lecturer");
            }

            // Programme Coordinator: Ebrahim Jacobs
            var coordinatorUser = await userManager.FindByEmailAsync("ebrahim.jacobs@newlands.ac.za");
            if (coordinatorUser == null)
            {
                coordinatorUser = new AppUser
                {
                    UserName = "ebrahim.jacobs",
                    Email = "ebrahim.jacobs@newlands.ac.za",
                    FullName = "Ebrahim Jacobs",
                    Role = "ProgrammeCoordinator"
                };
                await userManager.CreateAsync(coordinatorUser, "Password123");
                await userManager.AddToRoleAsync(coordinatorUser, "ProgrammeCoordinator");
            }

            // Academic Manager: Janet Du Plessis
            var managerUser = await userManager.FindByEmailAsync("janet.duplessis@newlands.ac.za");
            if (managerUser == null)
            {
                managerUser = new AppUser
                {
                    UserName = "janet.duplessis",
                    Email = "janet.duplessis@newlands.ac.za",
                    FullName = "Janet Du Plessis",
                    Role = "AcademicManager"
                };
                await userManager.CreateAsync(managerUser, "Password123");
                await userManager.AddToRoleAsync(managerUser, "AcademicManager");
            }

            // HR User
            var hrUser = await userManager.FindByEmailAsync("hr@newlands.ac.za");
            if (hrUser == null)
            {
                hrUser = new AppUser
                {
                    UserName = "hr.admin",
                    Email = "hr@newlands.ac.za",
                    FullName = "HR Administrator",
                    Role = "HR"
                };
                await userManager.CreateAsync(hrUser, "Password123");
                await userManager.AddToRoleAsync(hrUser, "HR");
            }
        }
    }
}
