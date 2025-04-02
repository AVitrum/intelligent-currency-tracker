using System.Globalization;
using Domain.Common;

namespace Domain.Entities;

public class Rate : BaseEntity
{
    private readonly DateTime _date;

    public required Guid CurrencyId { get; init; }
    public Currency? Currency { get; init; }
    public decimal Value { get; init; }
    public decimal ValueCompareToPrevious { get; set; }
    public int Year => Date.Year;
    public int Month => Date.Month;
    public int Day => Date.Day;
    public DayOfWeek DayOfWeek => Date.DayOfWeek;

    public int WeekNumber => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
        Date,
        CalendarWeekRule.FirstFourDayWeek,
        DayOfWeek.Monday);

    public bool IsHoliday { get; set; }

    public DateTime Date
    {
        get => _date;
        init => _date = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}