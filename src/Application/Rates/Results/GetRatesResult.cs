using System.Text.Json.Serialization;
using Domain.Common;
using Shared.Dtos;

namespace Application.Rates.Results;

public class GetRatesResult : BaseResult
{
    private GetRatesResult(bool success, IEnumerable<string> errors, IEnumerable<RateDto> rates) : base(success, errors)
    {
        Rates = rates;
    }

    [JsonInclude]
    public IEnumerable<RateDto> Rates { get; set; }

    public static GetRatesResult SuccessResult(IEnumerable<RateDto> rates)
    {
        return new GetRatesResult(true, [], rates);
    }
}