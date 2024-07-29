using HeulyAPI.Data;
using HeulyAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace HeulyAPI.IntegrationTests
{
    public static class SeedData
    {
        public static async Task Initialize(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            if (!userManager.Users.Any())
            {
                var user = new AppUser { UserName = "testuser", Email = "test@example.com" };
                await userManager.CreateAsync(user, "Test@123");
                await userManager.AddToRoleAsync(user, "User");
            }

            context.SaveChanges();
        }
    }

}
