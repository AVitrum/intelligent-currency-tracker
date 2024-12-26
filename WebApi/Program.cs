using Application;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using WebApi;
using WebApi.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCookiePolicy();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();