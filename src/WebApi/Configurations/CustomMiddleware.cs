using Microsoft.AspNetCore.Diagnostics;
using Serilog;

namespace WebApi.Configurations;

public static class CustomMiddleware
{
    public static void UseCustomMiddlewares(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseStaticFiles();
        app.UseCors("DevUI");
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
            var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();

            if (exceptionFeature?.Error != null)
            {
                await exceptionHandler.TryHandleAsync(context, exceptionFeature.Error, context.RequestAborted);
            }
        });
    }
}