using Application;
using Application.Common.Interfaces.Utils;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using WebApi.Configurations;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string defaultDirectory = EnvironmentSetup.ConfigureEnvironment(builder);
LoggingConfiguration.ConfigureLogging(builder, defaultDirectory);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSession();
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddCustomCorsPolicy();

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationResultHandler>();
builder.Services.AddScoped<IRateWebSocketHandler, RateWebSocketHandler>();

builder.Services
    .AddInfrastructure()
    .AddCustomAuthentication()
    .AddApplication();

builder.Services.AddSingleton<IExceptionHandler, CustomExceptionHandler>();
WebApplication app = builder.Build();

if (args.Length == 1 && args[0].Equals("seeddata", StringComparison.CurrentCultureIgnoreCase))
{
    // await UserSeeder.SeedRolesAsync(app);
    // await CurrencySeeder.SeedCurrenciesAsync(app);
    // await RateSeeder.SeedRatesAsync(app);
}

app.ConfigureWebSocket();
app.UseCustomMiddlewares();
app.Run();