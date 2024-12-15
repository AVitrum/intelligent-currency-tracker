namespace Application.Common.Models;

public class ExchangeRatePredictionDto
{
    public required string PreDate { get; set; }
    public required string CurrencyCode { get; set; }
}