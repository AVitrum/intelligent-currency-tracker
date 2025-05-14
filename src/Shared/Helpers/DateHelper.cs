using System.Globalization;
using Domain.Constants;

namespace Shared.Helpers;

public class DateHelper
{
    public static DateTime ParseDdMmYyyy(string date)
    {
        if (string.IsNullOrEmpty(date))
        {
            throw new ArgumentException("Date cannot be null or empty.", nameof(date));
        }

        DateTime dateTime = DateTime.ParseExact(
            date,
            DateConstants.DateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

        return dateTime;
    }

    public static string GetDateFormat()
    {
        return DateConstants.DateFormat;
    }
}