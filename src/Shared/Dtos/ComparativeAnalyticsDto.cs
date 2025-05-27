namespace Shared.Dtos;

public record ComparativeAnalyticsDto
{
    public required SingleCurrencyAnalyticsDto BaseCurrency { get; init; }
    public required SingleCurrencyAnalyticsDto ComparedCurrency { get; init; }
    public DateTime AnalysisStartDate { get; init; }
    public DateTime AnalysisEndDate { get; init; }
    public double CorrelationCoefficient { get; init; }
    public decimal AverageSpread { get; init; }
    public double SpreadStandardDeviation { get; init; }
}