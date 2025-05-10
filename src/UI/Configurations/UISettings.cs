namespace UI.Configurations;

public class UISettings : IUISettings
{
    public UISettings()
    {
        // Set this variable to true if you are running the application in a Docker container
        bool isDocker = true;
        ApiUrl = SetVariable(isDocker, "https://localhost:8001/api", "/api");
        WebSocketUrl = SetVariable(isDocker, "wss://localhost:8001/ws/rates", "/ws/rates");
    }

    public string ApiUrl { get; }
    public string WebSocketUrl { get; }

    private static string SetVariable(bool isDocker, string value, string dockerValue)
    {
        return isDocker ? dockerValue : value;
    }
}