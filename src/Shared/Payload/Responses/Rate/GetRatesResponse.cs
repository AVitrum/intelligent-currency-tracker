using System.Text.Json.Serialization;
using Domain.Common;
using Shared.Dtos;

namespace Shared.Payload.Responses.Rate;

public class GetRatesResponse : BaseResponse
{
    public GetRatesResponse(
        bool success,
        string message,
        int statusCode,
        IEnumerable<string> errors,
        IEnumerable<RateDto>? rates) : base(success, message, statusCode, errors)
    {
        Rates = rates;
    }

    [JsonInclude]
    public IEnumerable<RateDto>? Rates { get; set; }
}