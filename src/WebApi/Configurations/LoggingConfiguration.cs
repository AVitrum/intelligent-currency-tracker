using Serilog;
using Serilog.Events;

namespace WebApi.Configurations;

public static class LoggingConfiguration
{
    public static void ConfigureLogging(WebApplicationBuilder builder, string defaultDirectory)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File($"{defaultDirectory}/logs/api.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}