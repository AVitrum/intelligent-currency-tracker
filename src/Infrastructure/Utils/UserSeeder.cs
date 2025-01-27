using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Utils;

public static class UserSeeder
{
    public static async Task SeedUsersAsync(IApplicationBuilder applicationBuilder)
    {
        using var serviceScope = applicationBuilder.ApplicationServices.CreateScope();
        var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@gmail.com",
            PhoneNumber = "123456789",
            EmailConfirmed = true,
            CreationMethod = UserCreationMethod.EMAIL
        };

        var creationResult = await userManager.CreateAsync(adminUser);
        if (!creationResult.Succeeded)
            throw new Exception("Failed to create admin user");

        var roleResult = await userManager.AddToRoleAsync(adminUser, UserRole.USER.ToString());
        if (!roleResult.Succeeded)
            throw new Exception("Failed to add user to role");

        roleResult = await userManager.AddToRoleAsync(adminUser, UserRole.ADMIN.ToString());
        if (!roleResult.Succeeded)
            throw new Exception("Failed to add user to role");

        //TODO: Remove this line in production
        var passwordResult = await userManager.AddPasswordAsync(adminUser, "Admin123!");
        if (!passwordResult.Succeeded)
            throw new Exception("Failed to set password for admin user");

        var user = new ApplicationUser
        {
            UserName = "user",
            Email = "user@gmail.com",
            PhoneNumber = "123456789",
            EmailConfirmed = true,
            CreationMethod = UserCreationMethod.EMAIL
        };

        creationResult = await userManager.CreateAsync(user);
        if (!creationResult.Succeeded)
            throw new Exception("Failed to create user");

        roleResult = await userManager.AddToRoleAsync(user, UserRole.USER.ToString());
        if (!roleResult.Succeeded)
            throw new Exception("Failed to add user to role");

        passwordResult = await userManager.AddPasswordAsync(user, "User123!");
        if (!passwordResult.Succeeded)
            throw new Exception("Failed to set password for user");
    }

    public static async Task SeedRolesAsync(IApplicationBuilder applicationBuilder)
    {
        using var serviceScope = applicationBuilder.ApplicationServices.CreateScope();
        var roleManager =
            serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roleNames = ["Admin", "User"];

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist) await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}