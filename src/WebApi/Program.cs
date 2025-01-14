using Application;
using DotNetEnv;
using Infrastructure;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using Serilog.Events;
using WebApi;
using WebApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var defaultDirectory = "/app";

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
{
    defaultDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..", "..");
    Env.Load(Path.Combine(defaultDirectory, ".env.development"));
    builder.Configuration.AddEnvironmentVariables();
}

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File($"{defaultDirectory}/logs/api.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

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

var app = builder.Build();

if (args.Length == 1 && args[0].Equals("seeddata", StringComparison.CurrentCultureIgnoreCase))
    // await CurrencySeeder.SeedCurrenciesAsync(app);
    await RoleSeeder.SeedRolesAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
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
    var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
    if (exceptionFeature?.Error != null)
        await exceptionHandler.TryHandleAsync(context, exceptionFeature.Error, context.RequestAborted);
});

app.Run();