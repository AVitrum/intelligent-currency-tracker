using Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace WebApi.Configurations;

public static class CustomMiddleware
{
    public static async Task MigrateDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
            {
                await dbContext.Database.MigrateAsync();
            }
            
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }
    
    public static void UseCustomMiddlewares(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseStaticFiles();

        app.UseCors("AllowedCors");

        app.UseSerilogRequestLogging();
        app.UseExceptionHandler("/error");
        app.UseRouting();
        app.UseCookiePolicy();
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.MapControllers();

        app.Map("/error", async (HttpContext context, IExceptionHandler exceptionHandler) =>
        {
            IExceptionHandlerFeature? exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();

            if (exceptionFeature?.Error != null)
            {
                await exceptionHandler.TryHandleAsync(context, exceptionFeature.Error, context.RequestAborted);
            }
        });
    }
}