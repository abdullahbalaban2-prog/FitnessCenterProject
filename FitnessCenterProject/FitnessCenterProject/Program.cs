using FitnessCenterProject.Data;
using FitnessCenterProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace FitnessCenterProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // DB
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Identity
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequiredLength = 3;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // Localization
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            builder.Services
                .AddControllersWithViews()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();
            builder.Services.AddRazorPages();
            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            var supportedCultures = new[]
            {
                new CultureInfo("tr"),
                new CultureInfo("en")
            };

            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("tr"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };

            localizationOptions.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());

            app.UseRequestLocalization(localizationOptions);

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            await SeedData.InitializeAsync(app.Services);

            app.Run();
        }
    }
}
