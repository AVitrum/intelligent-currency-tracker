using Blazored.Toast;
using DevUI;
using DevUI.Configurations;
using DotNetEnv;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IDevUISettings, DevUISettings>();
builder.Services.AddBlazoredToast();

Env.Load("./.env");

await builder.Build().RunAsync();