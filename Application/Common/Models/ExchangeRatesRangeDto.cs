using Domain.Enums;

namespace Application.Common.Models;

public class ExchangeRatesRangeDto
{
    public required string StartDate { get; set; }
    public required string EndDate { get; set; }
    public Currency? Currency { get; set; }
    
    public bool TryGetDateRange(out DateTime start, out DateTime end)
    {
        start = default;
        end = default;
        return DateTime.TryParseExact(StartDate, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out start) &&
               DateTime.TryParseExact(EndDate, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out end);
    }
}
