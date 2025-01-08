using System.Text.Json.Serialization;
using Application.Common.Validation;

namespace Application.Common.Payload.Requests;

public class ExchangeRateRequest
{
    [DateFormat] public required string StartDateString { get; init; }
    [DateFormat] public required string EndDateString { get; init; }
    public string? Currency { get; init; }

    private DateTime _start;
    private DateTime _end;

    [JsonIgnore]
    public DateTime Start
    {
        get => _start;
        set
        {
            _start = value;
            _start = DateTime.ParseExact(StartDateString, "dd.MM.yyyy", null);
        }
    }

    [JsonIgnore]
    public DateTime End
    {
        get => _end;
        set
        {
            _end = value;
            _end = DateTime.ParseExact(EndDateString, "dd.MM.yyyy", null);
        }
    }
}