using Domain.Common;
using Shared.Dtos;

namespace Shared.Payload.Responses.Rate;

public class GetCrossRatesResponse : BaseResponse
{
    public IEnumerable<CrossRateDto> CrossRates { get; }
    
    public GetCrossRatesResponse(bool success, string message, int statusCode, IEnumerable<string> errors, IEnumerable<CrossRateDto> crossRates) : base(success, message, statusCode, errors)
    {
        CrossRates = crossRates;
    }
}