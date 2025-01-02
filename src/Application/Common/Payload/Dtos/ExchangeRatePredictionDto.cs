namespace Application.Common.Payload.Dtos;

public class ExchangeRatePredictionDto
{
    public required string PreDate { get; set; }
    public required string CurrencyCode { get; set; }
}