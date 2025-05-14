using Application.Common.Interfaces.Services;
using Application.Rates.Results;
using Domain.Common;
using Infrastructure.Summary.Results;
using Shared.Dtos;

namespace Infrastructure.Summary;

public class SummaryService : ISummaryService
{
    private readonly IRateService _rateService;

    public SummaryService(IRateService rateService)
    {
        _rateService = rateService;
    }

    public async Task<BaseResult> GenerateSummaryAsync(DateTime startDate, DateTime endDate, string currencyCode)
    {
        BaseResult result = await _rateService.GetRatesAsync(startDate, endDate, currencyCode, 0, 0);
        if (result is GetRatesResult getRatesResult)
        {
            List<RateDto> rates = ((List<RateDto>)getRatesResult.Rates)
                .OrderBy(x => x.Date)
                .ToList();

            if (rates.Count == 0)
            {
                return BaseResult.FailureResult(["No rates found for the selected period."]);
            }

            decimal average = rates.Average(x => x.Value);
            decimal min = rates.Min(x => x.Value);
            decimal max = rates.Max(x => x.Value);
            decimal first = rates.First().Value;
            decimal last = rates.Last().Value;
            decimal absoluteChange = last - first;
            decimal relativeChange = first != 0 ? (last - first) / first * 100 : 0;

            double stdDev = Math.Sqrt(rates.Average(r => Math.Pow((double)(r.Value - average), 2)));

            double coeffOfVariation = average != 0 ? stdDev / (double)average * 100 : 0;

            int increases = 0;
            int decreases = 0;
            for (int i = 1; i < rates.Count; i++)
                if (rates[i].Value > rates[i - 1].Value)
                {
                    increases++;
                }
                else if (rates[i].Value < rates[i - 1].Value)
                {
                    decreases++;
                }

            SummaryDto summary = new SummaryDto(
                average,
                min,
                max,
                first,
                last,
                absoluteChange,
                relativeChange,
                stdDev,
                coeffOfVariation,
                increases,
                decreases);

            return GenerateSummaryResult.SuccessResult(summary);
        }

        return BaseResult.FailureResult(["Unable to retrieve rates."]);
    }
}