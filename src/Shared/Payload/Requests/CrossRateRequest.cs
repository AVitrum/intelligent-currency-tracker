namespace Shared.Payload.Requests;

public class CrossRateRequest : ExchangeRateRequest
{
    public string? Currency2 { get; init; }
}