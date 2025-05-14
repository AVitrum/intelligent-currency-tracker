namespace Shared.Dtos;

public class SummaryDto
{
    private decimal _absoluteChange;
    private decimal _average;
    private double _coefficientOfVariation;
    private decimal _first;
    private decimal _last;
    private decimal _max;
    private decimal _min;
    private decimal _relativeChangePercent;
    private double _standardDeviation;

    public SummaryDto(
        decimal average,
        decimal min,
        decimal max,
        decimal first,
        decimal last,
        decimal absoluteChange,
        decimal relativeChangePercent,
        double standardDeviation,
        double coefficientOfVariation,
        int daysIncreased,
        int daysDecreased)
    {
        Average = average;
        Min = min;
        Max = max;
        First = first;
        Last = last;
        AbsoluteChange = absoluteChange;
        RelativeChangePercent = relativeChangePercent;
        StandardDeviation = standardDeviation;
        CoefficientOfVariation = coefficientOfVariation;
        DaysIncreased = daysIncreased;
        DaysDecreased = daysDecreased;
    }

    public decimal Average
    {
        get => _average;
        private set => _average = Math.Round(value, 3);
    }

    public decimal Min
    {
        get => _min;
        private set => _min = Math.Round(value, 3);
    }

    public decimal Max
    {
        get => _max;
        private set => _max = Math.Round(value, 3);
    }

    public decimal First
    {
        get => _first;
        private set => _first = Math.Round(value, 3);
    }

    public decimal Last
    {
        get => _last;
        private set => _last = Math.Round(value, 3);
    }

    public decimal AbsoluteChange
    {
        get => _absoluteChange;
        private set => _absoluteChange = Math.Round(value, 3);
    }

    public decimal RelativeChangePercent
    {
        get => _relativeChangePercent;
        private set => _relativeChangePercent = Math.Round(value, 3);
    }

    public double StandardDeviation
    {
        get => _standardDeviation;
        private set => _standardDeviation = Math.Round(value, 3);
    }

    public double CoefficientOfVariation
    {
        get => _coefficientOfVariation;
        private set => _coefficientOfVariation = Math.Round(value, 3);
    }

    public int DaysIncreased { get; set; }
    public int DaysDecreased { get; set; }
}