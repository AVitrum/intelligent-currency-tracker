using DevUI.Interfaces;

namespace DevUI.Configurations;

public class DevUISettings : IDevUISettings
{
    public DevUISettings()
    {
        // Set this variable to true if you are running the application in a Docker container
        bool isDocker = false;
        ApiUrl = SetVariable(isDocker, "https://localhost:8001/api", "/api");
    }

    public string ApiUrl { get; }

    private static string SetVariable(bool isDocker, string value, string dockerValue)
    {
        return isDocker ? dockerValue : value;
    }
}