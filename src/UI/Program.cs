using Blazored.Toast;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UI;
using UI.Common.Interfaces;
using UI.Configurations;
using UI.Services;
using IConfiguration = UI.Common.Interfaces.IConfiguration;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddBlazoredToast();

builder.Services.AddScoped<IConfiguration, Configuration>();
builder.Services.AddScoped<IHttpClientService, HttpClientService>();
builder.Services.AddScoped<IUserSettingsService, UserSettingsServiceService>();
builder.Services.AddSingleton<WebSocketService>();

await builder.Build().RunAsync();