using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebATB.Data;
using WebATB.Data.Entities.Idenity;
using WebATB.Data.Entities.Identity;

namespace WebATB.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task SeedDataAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<RoleEntity>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();
        var db = scope.ServiceProvider.GetRequiredService<AppATBDbContext>();

        // Переконуємось, що БД існує
        await db.Database.MigrateAsync();

        string admin = "Admin";
        string user = "User";
        // ===== Roles =====
        if (!await roleManager.RoleExistsAsync(admin))
        {
            await roleManager.CreateAsync(new RoleEntity(admin));
        }

        if (!await roleManager.RoleExistsAsync(user))
        {
            await roleManager.CreateAsync(new RoleEntity(user));
        }

        // ===== Admin User =====
        var adminEmail = "admin@test.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new UserEntity
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Іван",
                LastName = "Малюк",
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, admin);
            }
        }
    }
}
