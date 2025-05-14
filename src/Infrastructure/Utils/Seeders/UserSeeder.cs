using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Utils.Seeders;

public static class UserSeeder
{
    public static async Task SeedUsersAsync(IApplicationBuilder applicationBuilder)
    {
        using IServiceScope serviceScope = applicationBuilder.ApplicationServices.CreateScope();
        UserManager<ApplicationUser> userManager =
            serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        ApplicationUser adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@gmail.com",
            PhoneNumber = "123456789",
            EmailConfirmed = true,
            CreationMethod = UserCreationMethod.EMAIL
        };

        IdentityResult creationResult = await userManager.CreateAsync(adminUser);
        if (!creationResult.Succeeded)
        {
            throw new Exception("Failed to create admin user");
        }

        IdentityResult roleResult = await userManager.AddToRoleAsync(adminUser, UserRole.USER.ToString());
        if (!roleResult.Succeeded)
        {
            throw new Exception("Failed to add user to role");
        }

        roleResult = await userManager.AddToRoleAsync(adminUser, UserRole.ADMIN.ToString());
        if (!roleResult.Succeeded)
        {
            throw new Exception("Failed to add user to role");
        }

        //TODO: Remove this line in production
        IdentityResult passwordResult = await userManager.AddPasswordAsync(adminUser, "Admin123!");
        if (!passwordResult.Succeeded)
        {
            throw new Exception("Failed to set password for admin user");
        }

        ApplicationUser user = new ApplicationUser
        {
            UserName = "user",
            Email = "user@gmail.com",
            PhoneNumber = "123456789",
            EmailConfirmed = true,
            CreationMethod = UserCreationMethod.EMAIL
        };

        creationResult = await userManager.CreateAsync(user);
        if (!creationResult.Succeeded)
        {
            throw new Exception("Failed to create user");
        }

        roleResult = await userManager.AddToRoleAsync(user, UserRole.USER.ToString());
        if (!roleResult.Succeeded)
        {
            throw new Exception("Failed to add user to role");
        }

        passwordResult = await userManager.AddPasswordAsync(user, "User123!");
        if (!passwordResult.Succeeded)
        {
            throw new Exception("Failed to set password for user");
        }
    }

    public static async Task SeedRolesAsync(IApplicationBuilder applicationBuilder)
    {
        using IServiceScope serviceScope = applicationBuilder.ApplicationServices.CreateScope();
        RoleManager<IdentityRole> roleManager =
            serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roleNames = ["ADMIN", "USER"];

        foreach (string? roleName in roleNames)
        {
            bool roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}