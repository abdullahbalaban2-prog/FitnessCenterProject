using FitnessCenterProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Migration'ları uygula
            await context.Database.MigrateAsync();

            // Roller
            var adminRole = "Admin";
            var memberRole = "Member";

            if (!await roleManager.RoleExistsAsync(adminRole))
                await roleManager.CreateAsync(new IdentityRole(adminRole));

            if (!await roleManager.RoleExistsAsync(memberRole))
                await roleManager.CreateAsync(new IdentityRole(memberRole));

            // Admin kullanıcı
            var adminEmail = "b231210049@sakarya.edu.tr";
            var adminPassword = "sau";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = "Sistem Yöneticisi"
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                {
                    throw new Exception("Admin kullanıcısı oluşturulamadı: " +
                        string.Join(" | ", result.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, adminRole))
                await userManager.AddToRoleAsync(adminUser, adminRole);

            
        }
    }
}