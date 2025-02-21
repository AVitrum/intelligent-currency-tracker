using Application.Common.Interfaces.Utils;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Configuration;

public class AppSettings : IAppSettings
{
    private readonly IConfiguration _configuration;
    private readonly bool _isDocker;

    public AppSettings(IConfiguration configuration)
    {
        _configuration = configuration;
        _isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

        Initialize();
    }

    public string DbConnectionString { get; private set; } = null!;
    public string JwtKey { get; private set; } = null!;
    public string JwtIssuer { get; private set; } = null!;
    public string JwtAudience { get; private set; } = null!;
    public string GoogleClientId { get; private set; } = null!;
    public string GoogleClientSecret { get; private set; } = null!;
    public string GmailEmail { get; private set; } = null!;
    public string GmailPassword { get; private set; } = null!;
    public string ModelUrl { get; private set; } = null!;
    public string NbuUrl { get; private set; } = null!;
    public string KafkaHost { get; private set; } = null!;

    public bool IsDocker()
    {
        return _isDocker;
    }

    private void Initialize()
    {
        DbConnectionString = GetConnectionString();
        JwtKey = GetConfigurationValue("JWT_KEY");
        JwtIssuer = GetConfigurationValue("JWT_ISSUER");
        JwtAudience = GetConfigurationValue("JWT_AUDIENCE");
        GoogleClientId = GetConfigurationValue("GOOGLE_CLIENT_ID");
        GoogleClientSecret = GetConfigurationValue("GOOGLE_CLIENT_SECRET");
        GmailEmail = GetConfigurationValue("GMAIL_EMAIL");
        GmailPassword = GetConfigurationValue("GMAIL_PASSWORD");
        ModelUrl = GetConfigurationValue("MODEL_URL");
        NbuUrl = GetConfigurationValue("NBU_URL");
        KafkaHost = GetConfigurationValue("KAFKA_HOST");
    }

    private string GetConnectionString()
    {
        return
            $"Host={_configuration["DB_HOST"]};Port={_configuration["DB_PORT"]};Database={_configuration["DB_NAME"]};Username={_configuration["DB_USER"]};Password={_configuration["DB_PASSWORD"]}";
    }

    private string GetConfigurationValue(string key)
    {
        var value = _configuration[key];
        if (string.IsNullOrEmpty(value)) throw new Exception($"{key} cannot be null or empty");

        return value;
    }
}