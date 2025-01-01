using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public class AppSettings : IAppSettings
{
    public string DbConnectionString { get; private set; } = null!;
    public string JwtKey { get; private set; } = null!;
    public string JwtIssuer { get; private set; } = null!;
    public string JwtAudience { get; private set; } = null!;
    public string GoogleClientId { get; private set; } = null!;
    public string GoogleClientSecret { get; private set; } = null!;
    public string ModelUrl { get; private set; } = null!;
    public string PrivateBankUrl { get; private set; } = null!;
    public string KafkaHost { get; private set; } = null!;

    private readonly IConfiguration _configuration;
    private readonly bool _isDocker;

    public AppSettings(IConfiguration configuration)
    {
        _configuration = configuration;
        _isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

        Initialize();
    }
    
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
        ModelUrl = GetConfigurationValue("MODEL_URL");
        PrivateBankUrl = GetConfigurationValue("PRIVATE_BANK_URL");
        KafkaHost = GetConfigurationValue("KAFKA_HOST");
    }

    private string GetConnectionString()
    {
        return
            $"Host={_configuration["DB_HOST"]};Port={_configuration["DB_PORT"]};Database={_configuration["DB_NAME"]};Username={_configuration["DB_USER"]};Password={_configuration["DB_PASSWORD"]}";
    }

    private string GetConfigurationValue(string key)
    {
        string? value = _configuration[key];
        if (string.IsNullOrEmpty(value))
        {
            throw new Exception($"{key} cannot be null or empty");
        }
        
        return value;
    }
}