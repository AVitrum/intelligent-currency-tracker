using System.Net.WebSockets;
using System.Text.Json;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Application.Rates.Results;
using Domain.Common;
using Shared.Payload.Requests;
using Shared.Payload.Responses.Rate;

namespace WebApi.Configurations;

public class RateWebSocketHandler : IRateWebSocketHandler
{
    private readonly ILogger<RateWebSocketHandler> _logger;
    private readonly IRateService _rateService;

    public RateWebSocketHandler(IRateService rateService, ILogger<RateWebSocketHandler> logger)
    {
        _rateService = rateService;
        _logger = logger;
    }

    public async Task HandleAsync(WebSocket socket)
    {
        byte[] buffer = new byte[128 * 1024 * 1024];
        _logger.LogInformation("Started handling WebSocket connection.");

        while (socket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result =
                await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            _logger.LogDebug("Received message of type: {MessageType}", result.MessageType);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                string json = GetStringFromBuffer(buffer, result.Count);

                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("Empty JSON received, closing socket.");
                    await CloseSocketWithMessage(socket, WebSocketCloseStatus.InvalidPayloadData, "Empty request");
                    return;
                }

                await ProcessTextMessageAsync(socket, json);
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                _logger.LogInformation("Received close message.");
                await CloseSocketWithMessage(socket, WebSocketCloseStatus.NormalClosure, "Closing");
            }
        }
    }

    private string GetStringFromBuffer(byte[] buffer, int count)
    {
        return Encoding.UTF8.GetString(buffer, 0, count);
    }

    private async Task CloseSocketWithMessage(WebSocket socket, WebSocketCloseStatus status, string message)
    {
        _logger.LogInformation("Closing socket with status {Status} and message {Message}.", status, message);
        await socket.CloseAsync(status, message, CancellationToken.None);
    }

    private async Task ProcessTextMessageAsync(WebSocket socket, string json)
    {
        ExchangeRateRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<ExchangeRateRequest>(json);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize incoming JSON.");
            await SendResponseAsync(
                socket,
                new GetRatesResponse(
                    false,
                    "Invalid JSON format.",
                    StatusCodes.Status400BadRequest,
                    new List<string> { "Malformed JSON" },
                    null));
            return;
        }

        if (request is not null)
        {
            _logger.LogInformation("Processing request for currency {Currency}.", request.Currency);

            BaseResult serviceResult = await _rateService.GetRatesAsync(
                request.Start,
                request.End,
                request.Currency,
                request.Page,
                request.PageSize);

            if (serviceResult is not GetRatesResult ratesResult)
            {
                _logger.LogWarning("Service returned error: {@Errors}", serviceResult.Errors);
                await SendResponseAsync(
                    socket,
                    new GetRatesResponse(
                        false,
                        "Failed to retrieve rates.",
                        StatusCodes.Status400BadRequest,
                        serviceResult.Errors,
                        null));
                return;
            }

            if (!ratesResult.Rates.Any())
            {
                _logger.LogInformation("No rates found for currency {Currency}.", request.Currency);
                await SendResponseAsync(
                    socket,
                    new GetRatesResponse(
                        false,
                        "No rates found.",
                        StatusCodes.Status404NotFound,
                        new List<string> { "No rates found" },
                        null));
                return;
            }

            _logger.LogInformation("Successfully retrieved rates for currency {Currency}.", request.Currency);
            await SendResponseAsync(
                socket,
                new GetRatesResponse(
                    true,
                    "Rates retrieved successfully.",
                    StatusCodes.Status200OK,
                    new List<string>(),
                    ratesResult.Rates));
        }
    }

    private async Task SendResponseAsync(WebSocket socket, GetRatesResponse response)
    {
        string responseJson = JsonSerializer.Serialize(response);
        byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);
        _logger.LogDebug("Sending response");
        await socket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }
}