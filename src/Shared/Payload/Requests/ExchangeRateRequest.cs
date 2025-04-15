using System.Globalization;
using System.Text.Json.Serialization;
using Domain.Constants;
using Shared.Validation;

namespace Shared.Payload.Requests;

[DateRange]
public class ExchangeRateRequest
{
    private DateTime _end;
    private DateTime _start;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public required string StartDateString { get; init; }
    public required string EndDateString { get; set; }
    public string? Currency { get; init; }

    [JsonIgnore]
    public DateTime Start
    {
        get
        {
            if (_start == default)
            {
                _start = DateTime.ParseExact(
                    StartDateString,
                    DateConstants.DateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
                );
            }

            return _start;
        }
    }

    [JsonIgnore]
    public DateTime End
    {
        get
        {
            if (string.IsNullOrEmpty(EndDateString))
            {
                EndDateString = DateTime.UtcNow.ToString(DateConstants.DateFormat);
            }

            if (_end == default)
            {
                _end = DateTime.ParseExact(
                    EndDateString,
                    DateConstants.DateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
                );
            }

            return _end;
        }
    }
}