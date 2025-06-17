namespace Shared.Dtos;

public class CrossRateDto
{
    public DateTime Date { get; set; }
    public string SourceCurrencyCode { get; set; } = string.Empty;
    public string TargetCurrencyCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}