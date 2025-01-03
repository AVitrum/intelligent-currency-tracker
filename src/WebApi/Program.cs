using Application;
using DotNetEnv;
using Infrastructure;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Diagnostics;
using WebApi;
using WebApi.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
{
    Env.Load(Path.Combine("..", "..", ".env.development"));
    builder.Configuration.AddEnvironmentVariables();
}

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSession();
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

builder.Services
    .AddInfrastructure()
    .AddCustomAuthentication()
    .AddApplication();

builder.Services.AddSingleton<IExceptionHandler, CustomExceptionHandler>();

WebApplication app = builder.Build();

if (args.Length == 1 && args[0].Equals("seeddata", StringComparison.CurrentCultureIgnoreCase))
    await RoleSeeder.SeedRolesAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler("/error");
app.UseHttpsRedirection();
app.UseRouting();
app.UseCookiePolicy();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Map("/error", async (HttpContext context, IExceptionHandler exceptionHandler) =>
{
    IExceptionHandlerFeature? exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
    if (exceptionFeature?.Error != null)
        await exceptionHandler.TryHandleAsync(context, exceptionFeature.Error, context.RequestAborted);
});

app.Run();