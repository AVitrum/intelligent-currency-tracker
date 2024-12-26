using System.ComponentModel.DataAnnotations;
using Application.Common.Models;

namespace Application.Common.Validation;

public class DateFormatAttribute : ValidationAttribute
{
    private const string DateFormat = "dd.MM.yyyy";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var dto = (ExchangeRatesRangeDto)validationContext.ObjectInstance;
        if (DateTime.TryParseExact(dto.StartDateString, DateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime startDate) &&
            DateTime.TryParseExact(dto.EndDateString, DateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime endDate))
        {
            if (startDate <= endDate)
            {
                dto.Start = startDate;
                dto.End = endDate;
            }
            else
            {
                dto.Start = endDate;
                dto.End = startDate;
            }

            return ValidationResult.Success;
        }
        return new ValidationResult("Invalid date range or format");
    }
}