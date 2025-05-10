using System.Net.WebSockets;

namespace Application.Common.Interfaces.Utils;

public interface IRateWebSocketHandler
{
    Task HandleAsync(WebSocket socket);
}