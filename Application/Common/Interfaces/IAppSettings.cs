namespace Application.Common.Interfaces;

public interface IAppSettings
{
    string DbConnectionString { get; }
    string JwtKey { get; }
    string JwtIssuer { get; }
    string JwtAudience { get; }
    string GoogleClientId { get; }
    string GoogleClientSecret { get; }
    string ModelUrl { get; }
    string PrivateBankUrl { get; }
    bool IsDocker();
}