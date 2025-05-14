using System.Net.WebSockets;
using Application.Common.Interfaces.Utils;

namespace WebApi.Configurations;

public static class WebSocketConfiguration
{
    public static void ConfigureWebSocket(this WebApplication app)
    {
        app.UseWebSockets();

        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/ws/rates")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    IRateWebSocketHandler handler = context.RequestServices.GetRequiredService<IRateWebSocketHandler>();
                    await handler.HandleAsync(webSocket);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }
            else
            {
                await next();
            }
        });
    }
}