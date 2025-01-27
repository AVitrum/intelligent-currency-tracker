using System.Text.Json.Serialization;
using Domain.Constans;
using Shared.Validation;

namespace Shared.Payload;

public class ExchangeRateRequest
{
    private DateTime _end;
    private DateTime _start;

    [DateFormat] public required string StartDateString { get; init; }

    [DateFormat] public required string EndDateString { get; init; }

    public string? Currency { get; init; }

    [JsonIgnore]
    public DateTime Start
    {
        get => _start.ToUniversalTime();
        set
        {
            _start = value;
            _start = DateTime.ParseExact(StartDateString, DateConstants.DateFormat, null);
        }
    }

    [JsonIgnore]
    public DateTime End
    {
        get => _end.ToUniversalTime();
        set
        {
            _end = value;
            _end = DateTime.ParseExact(EndDateString, DateConstants.DateFormat, null);
        }
    }
}