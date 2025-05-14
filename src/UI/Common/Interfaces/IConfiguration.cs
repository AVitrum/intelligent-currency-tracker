namespace UI.Common.Interfaces;

public interface IConfiguration
{
    string ApiUrl { get; }
    string WebSocketUrl { get; }
}