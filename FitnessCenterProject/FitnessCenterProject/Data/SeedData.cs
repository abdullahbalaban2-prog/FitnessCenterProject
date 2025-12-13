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

            // ----------------------------------------
            // 1) Roller
            // ----------------------------------------
            var adminRole = "Admin";
            var memberRole = "Member";

            if (!await roleManager.RoleExistsAsync(adminRole))
                await roleManager.CreateAsync(new IdentityRole(adminRole));

            if (!await roleManager.RoleExistsAsync(memberRole))
                await roleManager.CreateAsync(new IdentityRole(memberRole));

            // ----------------------------------------
            // 2) Admin kullanıcı
            // ----------------------------------------
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
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }

            // ----------------------------------------
            // 3) Domain verileri (Salon, Hizmet, Eğitmen, TrainerService)
            //    Daha önce oluşturulmadıysa ekleyelim
            // ----------------------------------------

            if (!context.FitnessCenters.Any())
            {
                // SALON
                var merkez = new FitnessCenter
                {
                    Name = "Merkez Spor Salonu",
                    Address = "Sakarya / Merkez",
                    Description = "Örnek seed salonu",
                    WorkingHours = "08:00 - 22:00"
                };

                context.FitnessCenters.Add(merkez);
                await context.SaveChangesAsync();

                // HİZMETLER
                var serviceFitness = new Service
                {
                    Name = "Genel Fitness",
                    Description = "Ağırlık ve kondisyon çalışması",
                    DurationMinutes = 60,
                    Price = 250,
                    FitnessCenterId = merkez.Id
                };

                var serviceYoga = new Service
                {
                    Name = "Yoga",
                    Description = "Esneklik ve nefes odaklı yoga seansı",
                    DurationMinutes = 60,
                    Price = 300,
                    FitnessCenterId = merkez.Id
                };

                var servicePilates = new Service
                {
                    Name = "Pilates",
                    Description = "Duruş ve core kuvvetlendirme",
                    DurationMinutes = 60,
                    Price = 280,
                    FitnessCenterId = merkez.Id
                };

                context.Services.AddRange(serviceFitness, serviceYoga, servicePilates);
                await context.SaveChangesAsync();

                // EĞİTMENLER
                var trainerAli = new Trainer
                {
                    FirstName = "Ali",
                    LastName = "Güçlü",
                    Bio = "Genel fitness antrenörü",
                    Specialty = "Genel Fitness",
                    FitnessCenterId = merkez.Id
                };

                var trainerZeynep = new Trainer
                {
                    FirstName = "Zeynep",
                    LastName = "Demir",
                    Bio = "Yoga eğitmeni",
                    Specialty = "Yoga",
                    FitnessCenterId = merkez.Id
                };

                var trainerMelis = new Trainer
                {
                    FirstName = "Melis",
                    LastName = "Kaya",
                    Bio = "Pilates eğitmeni",
                    Specialty = "Pilates",
                    FitnessCenterId = merkez.Id
                };

                context.Trainers.AddRange(trainerAli, trainerZeynep, trainerMelis);
                await context.SaveChangesAsync();

                // TRAINER-SERVICE (çoktan çoğa eşleşmeler)
                // Ali → Genel Fitness
                // Zeynep → Yoga
                // Melis → Pilates
                context.TrainerServices.AddRange(
                    new TrainerService
                    {
                        TrainerId = trainerAli.Id,
                        ServiceId = serviceFitness.Id
                    },
                    new TrainerService
                    {
                        TrainerId = trainerZeynep.Id,
                        ServiceId = serviceYoga.Id
                    },
                    new TrainerService
                    {
                        TrainerId = trainerMelis.Id,
                        ServiceId = servicePilates.Id
                    }
                );

                await context.SaveChangesAsync();
            }

            // (İleride istersen buraya TrainerAvailability vs. seed de ekleyebiliriz)
        }
    }
}
