using Microsoft.AspNetCore.Identity;
using ObituaryApplication.Models;

namespace ObituaryApplication.Data
{
    public static class DbInitializer
    {
        public static async Task Seed(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            // 1️⃣ Create roles if they don't exist
            if (!await roleManager.RoleExistsAsync("admin"))
                await roleManager.CreateAsync(new IdentityRole("admin"));

            if (!await roleManager.RoleExistsAsync("user"))
                await roleManager.CreateAsync(new IdentityRole("user"));

            // 2️⃣ Create admin user
            var admin = await userManager.FindByEmailAsync("aa@aa.aa");
            if (admin == null)
            {
                admin = new IdentityUser
                {
                    UserName = "aa@aa.aa",
                    Email = "aa@aa.aa",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, "P@$$w0rd");
                await userManager.AddToRoleAsync(admin, "admin");
            }

            // 3️⃣ Create regular user
            var user = await userManager.FindByEmailAsync("uu@uu.uu");
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = "uu@uu.uu",
                    Email = "uu@uu.uu",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, "P@$$w0rd");
                await userManager.AddToRoleAsync(user, "user");
            }

            // 4️⃣ Create obituary records for the seeded users
            if (!context.Obituaries.Any())
            {
                var obituaries = new List<Obituary>
                {
                    new Obituary
                    {
                        FullName = "Admin User",
                        DOB = new DateTime(1980, 1, 1),
                        DOD = new DateTime(2023, 12, 31),
                        Biography = "Admin User (aa@aa.aa) was a dedicated system administrator who managed the obituary platform with care and precision. Known for their attention to detail and commitment to preserving memories for families.",
                        CreatorId = admin.Id  // Created by admin
                    },
                    new Obituary
                    {
                        FullName = "Regular User",
                        DOB = new DateTime(1985, 6, 15),
                        DOD = new DateTime(2024, 1, 15),
                        Biography = "Regular User (uu@uu.uu) was a valued member of the community who actively participated in the obituary system. They will be remembered for their kindness and dedication to preserving family histories.",
                        CreatorId = user.Id  // Created by regular user
                    }
                };

                context.Obituaries.AddRange(obituaries);
                await context.SaveChangesAsync();
            }
        }
    }
}
