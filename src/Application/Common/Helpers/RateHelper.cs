using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Utils;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Dtos;

namespace Application.Common.Helpers;

public class RateHelper : IRateHelper
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly ILogger<RateHelper> _logger;
    private readonly IMapper _mapper;
    private readonly IRateRepository _rateRepository;

    public RateHelper(
        ICurrencyRepository currencyRepository,
        IRateRepository rateRepository,
        IMapper mapper,
        ILogger<RateHelper> logger)
    {
        _currencyRepository = currencyRepository;
        _rateRepository = rateRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<Rate>> GetRatesAsync(DateTime start, DateTime end, int currencyR030)
    {
        IEnumerable<Rate> rates;
        if (currencyR030 == 0)
        {
            if (start.Date == end.Date)
            {
                rates = await _rateRepository.GetRangeAsync(start);
            }
            else
            {
                rates = await _rateRepository.GetRangeAsync(start, end);
            }
        }
        else
        {
            Currency currency = await _currencyRepository.GetByR030Async(currencyR030)
                                ?? throw new EntityNotFoundException<Currency>();

            rates = await _rateRepository.GetRangeAsync(start, end, currency);
        }

        return rates;
    }

    public IEnumerable<RateDto> ConvertRatesToDtoAsync(IEnumerable<Rate> rates)
    {
        List<RateDto> ratesDto = [];
        ratesDto.AddRange(rates.Select(rate => _mapper.Map<RateDto>(rate)));

        _logger.LogInformation("Successfully converted rates to DTO");
        return ratesDto;
    }

    public IEnumerable<CurrencyDto> ConvertCurrenciesToDtoAsync(IEnumerable<Currency> rates)
    {
        List<CurrencyDto> currenciesDto = [];
        currenciesDto.AddRange(rates.Select(currency => _mapper.Map<CurrencyDto>(currency)));

        _logger.LogInformation("Successfully converted currencies to DTO");
        return currenciesDto;
    }

    public SingleCurrencyAnalyticsDto AnalyzeCurrency(List<Rate> rates, string currencyCode)
    {
        if (rates == null || rates.Count < 2)
        {
            throw new ArgumentException("Required 2 or mode values for statistics.", nameof(rates));
        }

        List<decimal> rateValues = rates.Select(r => r.Value).ToList();

        decimal mean = rateValues.Average();
        decimal sumOfSquares = rateValues.Sum(val => (val - mean) * (val - mean));

        decimal variance = sumOfSquares / rateValues.Count;
        double stdDev = Math.Sqrt((double)variance);

        List<decimal> sortedRates = rateValues.OrderBy(v => v).ToList();
        decimal median = GetMedian(sortedRates);

        SingleCurrencyAnalyticsDto analysisDto = new SingleCurrencyAnalyticsDto
        {
            CurrencyPair = "UAH/" + currencyCode,
            MeanValue = Math.Round(mean, 4),
            MedianValue = Math.Round(median, 4),
            MaxValue = sortedRates[^1],
            MinValue = sortedRates[0],
            Range = Math.Round(sortedRates[^1] - sortedRates[0], 4),
            StandardDeviation = Math.Round(stdDev, 4),
            Variance = Math.Round((double)variance, 4),
            MovingAverages = CalculateMovingAverages(rateValues)
        };

        return analysisDto;
    }

    public ComparativeAnalyticsDto CompareCurrencies(
        List<SingleCurrencyAnalyticsDto> analysis,
        List<List<Rate>> ratesLists,
        DateTime start,
        DateTime end)
    {
        if (analysis == null || analysis.Count < 2 || ratesLists == null || ratesLists.Count < 2)
        {
            throw new ArgumentException("You need 2 or more currencies and their rates for comparison.",
                nameof(analysis));
        }

        SingleCurrencyAnalyticsDto baseAnalysis = analysis[0];
        SingleCurrencyAnalyticsDto comparedAnalysis = analysis[1];
        List<Rate> baseRates = ratesLists[0];
        List<Rate> comparedRates = ratesLists[1];

        (List<decimal> alignedBaseValues, List<decimal> alignedComparedValues) =
            AlignDataByDate(baseRates, comparedRates, start, end);

        if (alignedBaseValues.Count < 2)
        {
            return CreateEmptyComparativeDto(baseAnalysis, comparedAnalysis, start, end);
        }

        (decimal averageSpread, double spreadStdDev) =
            CalculateSpreadAnalytics(alignedBaseValues, alignedComparedValues);

        double correlation = CalculateCorrelation(alignedBaseValues, alignedComparedValues);

        ComparativeAnalyticsDto comparativeAnalytics = new ComparativeAnalyticsDto
        {
            BaseCurrency = baseAnalysis,
            ComparedCurrency = comparedAnalysis,
            AnalysisStartDate = start,
            AnalysisEndDate = end,
            CorrelationCoefficient = Math.Round(correlation, 4),
            AverageSpread = Math.Round(averageSpread, 4),
            SpreadStandardDeviation = Math.Round(spreadStdDev, 4)
        };

        return comparativeAnalytics;
    }

    private static (List<decimal> valuesA, List<decimal> valuesB) AlignDataByDate(
        List<Rate> ratesA,
        List<Rate> ratesB,
        DateTime start,
        DateTime end)
    {
        DateOnly startDateOnly = DateOnly.FromDateTime(start);
        DateOnly endDateOnly = DateOnly.FromDateTime(end);

        Dictionary<DateOnly, decimal> dictA = ratesA
            .Where(r => DateOnly.FromDateTime(r.Date) >= startDateOnly && DateOnly.FromDateTime(r.Date) <= endDateOnly)
            .ToDictionary(r => DateOnly.FromDateTime(r.Date), r => r.Value);

        Dictionary<DateOnly, decimal> dictB = ratesB
            .Where(r => DateOnly.FromDateTime(r.Date) >= startDateOnly && DateOnly.FromDateTime(r.Date) <= endDateOnly)
            .ToDictionary(r => DateOnly.FromDateTime(r.Date), r => r.Value);

        List<DateOnly> commonDates = dictA.Keys.Intersect(dictB.Keys).OrderBy(d => d).ToList();

        List<decimal> alignedValuesA = [];
        List<decimal> alignedValuesB = [];

        foreach (DateOnly date in commonDates)
        {
            alignedValuesA.Add(dictA[date]);
            alignedValuesB.Add(dictB[date]);
        }

        return (alignedValuesA, alignedValuesB);
    }

    private static (decimal average, double stdDev) CalculateSpreadAnalytics(
        IReadOnlyList<decimal> valuesA,
        IReadOnlyList<decimal> valuesB)
    {
        List<decimal> spreads = valuesA.Zip(valuesB, (a, b) => a - b).ToList();

        if (spreads.Count == 0)
        {
            return (0, 0);
        }

        decimal averageSpread = spreads.Average();
        decimal sumOfSquares = spreads.Sum(s => (s - averageSpread) * (s - averageSpread));
        decimal variance = sumOfSquares / spreads.Count;
        double spreadStandardDeviation = Math.Sqrt((double)variance);

        return (averageSpread, spreadStandardDeviation);
    }

    private static double CalculateCorrelation(List<decimal> valuesA, List<decimal> valuesB)
    {
        List<double> returnsA = [];
        List<double> returnsB = [];

        for (int i = 1; i < valuesA.Count; i++)
        {
            if (valuesA[i - 1] == 0 || valuesB[i - 1] == 0)
            {
                continue;
            }

            returnsA.Add((double)((valuesA[i] - valuesA[i - 1]) / valuesA[i - 1]));
            returnsB.Add((double)((valuesB[i] - valuesB[i - 1]) / valuesB[i - 1]));
        }

        if (returnsA.Count < 2)
        {
            return 0;
        }

        double meanA = returnsA.Average();
        double meanB = returnsB.Average();

        double sumOfProducts = 0;
        double sumOfSquaresA = 0;
        double sumOfSquaresB = 0;

        for (int i = 0; i < returnsA.Count; i++)
        {
            double devA = returnsA[i] - meanA;
            double devB = returnsB[i] - meanB;
            sumOfProducts += devA * devB;
            sumOfSquaresA += devA * devA;
            sumOfSquaresB += devB * devB;
        }

        double denominator = Math.Sqrt(sumOfSquaresA * sumOfSquaresB);
        if (denominator == 0)
        {
            return 0;
        }

        return sumOfProducts / denominator;
    }

    private static ComparativeAnalyticsDto CreateEmptyComparativeDto(
        SingleCurrencyAnalyticsDto baseAnalysis,
        SingleCurrencyAnalyticsDto comparedAnalysis,
        DateTime start,
        DateTime end)
    {
        return new ComparativeAnalyticsDto
        {
            BaseCurrency = baseAnalysis,
            ComparedCurrency = comparedAnalysis,
            AnalysisStartDate = start,
            AnalysisEndDate = end,
            CorrelationCoefficient = 0.0,
            AverageSpread = 0.0m,
            SpreadStandardDeviation = 0.0
        };
    }

    private static List<MovingAverageDto> CalculateMovingAverages(List<decimal> rateValues)
    {
        List<MovingAverageDto> movingAverages = [];
        int dataCount = rateValues.Count;

        foreach (int period in new[] { 5, 10, 20 })
        {
            if (dataCount < period)
            {
                continue;
            }

            decimal sma = rateValues.Skip(dataCount - period).Average();
            movingAverages.Add(new MovingAverageDto(period, Math.Round(sma, 4), MovingAverageType.SMA));

            decimal priorEma = sma;
            decimal multiplier = 2.0m / (period + 1);

            for (int i = dataCount - period + 1; i < dataCount; i++)
                priorEma = (rateValues[i] - priorEma) * multiplier + priorEma;
            movingAverages.Add(new MovingAverageDto(period, Math.Round(priorEma, 4), MovingAverageType.EMA));
        }

        return movingAverages;
    }

    private static decimal GetMedian(List<decimal> sortedValues)
    {
        int count = sortedValues.Count;
        int mid = count / 2;

        if (count % 2 == 0)
        {
            return (sortedValues[mid - 1] + sortedValues[mid]) / 2;
        }

        return sortedValues[mid];
    }
}