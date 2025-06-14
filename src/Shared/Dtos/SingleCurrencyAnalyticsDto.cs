namespace Shared.Dtos;

public record SingleCurrencyAnalyticsDto
{
    public string CurrencyPair { get; init; } = string.Empty;
    public decimal MeanValue { get; init; }
    public decimal MedianValue { get; init; }
    public decimal MaxValue { get; init; }
    public decimal MinValue { get; init; }
    public decimal Range { get; init; }
    public double StandardDeviation { get; init; }
    public double Variance { get; init; }
    public IReadOnlyList<MovingAverageDto> MovingAverages { get; init; } = [];
}