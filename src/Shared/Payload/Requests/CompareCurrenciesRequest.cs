using System.Text.Json.Serialization;
using Domain.Constants;
using Shared.Helpers;
using Shared.Validation;

namespace Shared.Payload.Requests;

[DateRange]
public class CompareCurrenciesRequest
{
    private DateTime _end;
    private DateTime _start;

    public required string StartDateString { get; init; }
    public required string EndDateString { get; set; }

    public required string CurrencyCode1 { get; set; }
    public required string CurrencyCode2 { get; set; }

    [JsonIgnore]
    public DateTime Start
    {
        get
        {
            if (_start == default)
            {
                _start = DateHelper.ParseDdMmYyyy(StartDateString);
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
                _end = DateHelper.ParseDdMmYyyy(EndDateString);
            }

            return _end;
        }
    }
}