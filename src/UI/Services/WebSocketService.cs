using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Shared.Payload.Requests;
using Shared.Payload.Responses.Rate;
using IConfiguration = UI.Common.Interfaces.IConfiguration;

namespace UI.Services;

public class WebSocketService : IAsyncDisposable
{
    private readonly Uri _serverUri;
    private readonly ClientWebSocket _socket = new ClientWebSocket();

    public WebSocketService(IServiceScopeFactory scopeFactory)
    {
        IConfiguration configuration = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        _serverUri = new Uri(configuration.WebSocketUrl);
    }

    public async ValueTask DisposeAsync()
    {
        if (_socket.State == WebSocketState.Open)
        {
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Dispose", CancellationToken.None);
        }

        _socket.Dispose();
    }

    public async Task ConnectAsync()
    {
        if (_socket.State != WebSocketState.Open)
        {
            await _socket.ConnectAsync(_serverUri, CancellationToken.None);
        }
    }

    public async Task<GetRatesResponse?> RequestRatesAsync(ExchangeRateRequest request)
    {
        string json = JsonSerializer.Serialize(request);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        await _socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

        byte[] receiveBuffer = new byte[128 * 1024 * 1024];
        WebSocketReceiveResult result = await _socket.ReceiveAsync(receiveBuffer, CancellationToken.None);
        string responseJson = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
        return JsonSerializer.Deserialize<GetRatesResponse>(responseJson);
    }
}