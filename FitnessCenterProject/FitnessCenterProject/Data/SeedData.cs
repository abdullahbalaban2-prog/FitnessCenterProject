using FitnessCenterProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace FitnessCenterProject.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<ApplicationDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();


            // database gerçekten var mı
            await context.Database.EnsureCreatedAsync();

            //roller
            string[] roles = new[] { "Admin", "Member" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.Roles.AnyAsync(r => r.Name == roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // admin
            var adminEmail = "b231210049@sakarya.edu.tr";
            var adminUserName = adminEmail;

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Name = "Admin Kullanıcı"
                };

                // şifre (sau) kuralı
                var createResult = await userManager.CreateAsync(adminUser, "sau");

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {

                }
            }
            await context.SaveChangesAsync();

        }
    }
}
