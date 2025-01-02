using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(IApplicationBuilder applicationBuilder)
    {
        using IServiceScope serviceScope = applicationBuilder.ApplicationServices.CreateScope();
        RoleManager<IdentityRole> roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roleNames = ["Admin", "User"];

        foreach (string roleName in roleNames)
        {
            bool roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}