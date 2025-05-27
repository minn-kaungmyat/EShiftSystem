using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using EShiftSystem.Data;
using EShiftSystem.Models;

namespace EShiftSystem.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            // Add roles and admin user
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roles = { "Admin", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create default admin user if not exists
            var adminEmail = "admin@eshift.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                };

                    var result = await userManager.CreateAsync(user, "Admin@123"); // Must meet Identity password requirements

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                    // Create the Admin entity linked to this ApplicationUser
                    var adminEntity = new Admin
                    {
                        ApplicationUserId = user.Id,
                        ApplicationUser = user
                       
                    };

                    context.Admins.Add(adminEntity);
                    await context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
