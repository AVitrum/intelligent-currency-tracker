using Application;
using Infrastructure;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Diagnostics;
using WebApi.Configurations;

var builder = WebApplication.CreateBuilder(args);

var defaultDirectory = EnvironmentSetup.ConfigureEnvironment(builder);
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

builder.Services
    .AddInfrastructure()
    .AddCustomAuthentication()
    .AddApplication();

builder.Services.AddSingleton<IExceptionHandler, CustomExceptionHandler>();
var app = builder.Build();

if (args.Length == 1 && args[0].Equals("seeddata", StringComparison.CurrentCultureIgnoreCase))
    // await CurrencySeeder.SeedCurrenciesAsync(app);
    await UserSeeder.SeedRolesAsync(app);

app.UseCustomMiddlewares();
app.Run();