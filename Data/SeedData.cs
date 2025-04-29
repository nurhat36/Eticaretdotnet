using ETicaret.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ETicaret.Data
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            var roleExist = await roleManager.RoleExistsAsync("Admin");

            // Eğer Admin rolü yoksa oluştur
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            string adminEmail = "admin@eticaret.com";
            string adminPassword = "Admin123!";

            // Admin kullanıcısı yoksa oluştur
            var user = await userManager.FindByEmailAsync(adminEmail);
            if (user == null)
            {
                user = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail
                };

                var result = await userManager.CreateAsync(user, adminPassword);
                if (result.Succeeded)
                {
                    // Kullanıcıyı Admin rolüne ekle
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
