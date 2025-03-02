namespace Application.Common.Interfaces.Utils;

public interface IAppSettings
{
    string DbConnectionString { get; }
    string AwsAccessKey { get; }
    string AwsSecretKey { get; }
    string AwsRegion { get; }
    string AwsBucket { get; }
    string AwsEndpoint { get; }
    string JwtKey { get; }
    string JwtIssuer { get; }
    string JwtAudience { get; }
    string GoogleClientId { get; }
    string GoogleClientSecret { get; }
    string GmailEmail { get; }
    string GmailPassword { get; }
    string ModelUrl { get; }
    string NbuUrl { get; }
    string KafkaHost { get; }
    bool IsDocker();
}