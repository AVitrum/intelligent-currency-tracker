using Application.Common.Payload.Dtos;
using Domain.Common;

namespace Application.Rates.Results;

public class GetRatesResult : BaseResult
{
    private GetRatesResult(bool success, IEnumerable<string> errors, IEnumerable<RateDto> rates) : base(success, errors)
    {
        Rates = rates;
    }

    public IEnumerable<RateDto> Rates { get; }

    public static GetRatesResult SuccessResult(IEnumerable<RateDto> rates)
    {
        return new GetRatesResult(true, Array.Empty<string>(), rates);
    }
}