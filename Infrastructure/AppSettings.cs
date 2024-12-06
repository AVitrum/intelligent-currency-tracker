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
        JwtKey = GetConfigurationValue("JWT_KEY", "Jwt:Key");
        JwtIssuer = GetConfigurationValue("JWT_ISSUER", "Jwt:Issuer");
        JwtAudience = GetConfigurationValue("JWT_AUDIENCE", "Jwt:Audience");
        GoogleClientId = GetConfigurationValue("GOOGLE_CLIENT_ID", "Authentication:Google:ClientId");
        GoogleClientSecret = GetConfigurationValue("GOOGLE_CLIENT_SECRET", "Authentication:Google:ClientSecret");
    }

    private string GetConnectionString()
    {
        return (_isDocker
            ? $"Host={_configuration["DB_HOST"]};Port={_configuration["DB_PORT"]};Database={_configuration["DB_NAME"]};Username={_configuration["DB_USER"]};Password={_configuration["DB_PASSWORD"]}"
            : _configuration.GetConnectionString("DefaultConnection"))!;
    }

    private string GetConfigurationValue(string dockerKey, string defaultKey)
    {
        var value = _isDocker ? _configuration[dockerKey] : _configuration[defaultKey];
        
        if (string.IsNullOrEmpty(value)) throw new Exception($"{( _isDocker ? dockerKey : defaultKey)} cannot be null or empty");
        
        return value;
    }
}